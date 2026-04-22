using System;
using System.Collections.Generic;
using System.IO;
using FenceNote.Models;
using Microsoft.Data.Sqlite;

namespace FenceNote.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly EncryptionService _encryptionService;

        public DatabaseService(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(folder, "FenceNote");
            Directory.CreateDirectory(appFolder);
            _dbPath = Path.Combine(appFolder, "notes.db");

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Vaults (
                    Id TEXT PRIMARY KEY,
                    Name TEXT,
                    Salt TEXT,
                    PasswordHash TEXT,
                    LockoutSeconds INTEGER DEFAULT 60
                );
                CREATE TABLE IF NOT EXISTS Notes (
                    Id TEXT PRIMARY KEY,
                    Title TEXT,
                    Content TEXT,
                    CreatedAt TEXT,
                    ModifiedAt TEXT
                )";
            command.ExecuteNonQuery();

            var checkColCmd = connection.CreateCommand();
            checkColCmd.CommandText = "PRAGMA table_info(Notes)";
            using var reader = checkColCmd.ExecuteReader();
            bool hasVaultId = false;
            bool hasOpenedAt = false;
            while (reader.Read())
            {
                string colName = reader.GetString(1);
                if (colName == "VaultId") hasVaultId = true;
                if (colName == "OpenedAt") hasOpenedAt = true;
            }
            reader.Close();

            if (!hasVaultId)
            {
                var alterCmd = connection.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Notes ADD COLUMN VaultId TEXT";
                alterCmd.ExecuteNonQuery();
            }

            if (!hasOpenedAt)
            {
                var alterCmd = connection.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Notes ADD COLUMN OpenedAt TEXT";
                alterCmd.ExecuteNonQuery();

                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = "UPDATE Notes SET OpenedAt = ModifiedAt WHERE OpenedAt IS NULL";
                updateCmd.ExecuteNonQuery();
            }

            var checkVaultColCmd = connection.CreateCommand();
            checkVaultColCmd.CommandText = "PRAGMA table_info(Vaults)";
            using var vaultReader = checkVaultColCmd.ExecuteReader();
            bool hasLockout = false;
            while (vaultReader.Read())
            {
                if (vaultReader.GetString(1) == "LockoutSeconds") hasLockout = true;
            }
            vaultReader.Close();

            if (!hasLockout)
            {
                var alterVaultCmd = connection.CreateCommand();
                alterVaultCmd.CommandText = "ALTER TABLE Vaults ADD COLUMN LockoutSeconds INTEGER DEFAULT 60";
                alterVaultCmd.ExecuteNonQuery();
            }

            EnsureDummyNoteExists(connection);
        }

        private void EnsureDummyNoteExists(SqliteConnection connection)
        {
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Notes";
            var count = (long)(checkCommand.ExecuteScalar() ?? 0);

            if (count == 0)
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                    INSERT INTO Notes (Id, Title, Content, CreatedAt, ModifiedAt, OpenedAt, VaultId)
                    VALUES ($id, $title, $content, $createdAt, $modifiedAt, $openedAt, NULL)";

                string now = DateTime.UtcNow.ToString("o");
                insertCommand.Parameters.AddWithValue("$id", Guid.NewGuid().ToString());
                insertCommand.Parameters.AddWithValue("$title", "Welcome to FenceNote");
                insertCommand.Parameters.AddWithValue("$content", "This is your first public note. It is stored locally on your machine in plaintext. Create a Vault to store encrypted notes.");
                insertCommand.Parameters.AddWithValue("$createdAt", now);
                insertCommand.Parameters.AddWithValue("$modifiedAt", now);
                insertCommand.Parameters.AddWithValue("$openedAt", now);

                insertCommand.ExecuteNonQuery();
            }
        }

        public IEnumerable<Vault> GetAllVaults()
        {
            var vaults = new List<Vault>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Salt, PasswordHash, LockoutSeconds FROM Vaults ORDER BY Name ASC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                vaults.Add(new Vault
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Salt = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    LockoutSeconds = reader.IsDBNull(4) ? 60 : reader.GetInt32(4),
                    IsUnlocked = false
                });
            }

            return vaults;
        }

        public void SaveVault(Vault vault)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Vaults (Id, Name, Salt, PasswordHash, LockoutSeconds)
                VALUES ($id, $name, $salt, $hash, $lockout)
                ON CONFLICT(Id) DO UPDATE SET
                    Name = excluded.Name,
                    Salt = excluded.Salt,
                    PasswordHash = excluded.PasswordHash,
                    LockoutSeconds = excluded.LockoutSeconds";

            command.Parameters.AddWithValue("$id", vault.Id);
            command.Parameters.AddWithValue("$name", vault.Name);
            command.Parameters.AddWithValue("$salt", vault.Salt);
            command.Parameters.AddWithValue("$hash", vault.PasswordHash);
            command.Parameters.AddWithValue("$lockout", vault.LockoutSeconds);

            command.ExecuteNonQuery();
        }

        public void DeleteVault(string id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            var deleteNotesCmd = connection.CreateCommand();
            deleteNotesCmd.CommandText = "DELETE FROM Notes WHERE VaultId = $id";
            deleteNotesCmd.Parameters.AddWithValue("$id", id);
            deleteNotesCmd.ExecuteNonQuery();

            var deleteVaultCmd = connection.CreateCommand();
            deleteVaultCmd.CommandText = "DELETE FROM Vaults WHERE Id = $id";
            deleteVaultCmd.Parameters.AddWithValue("$id", id);
            deleteVaultCmd.ExecuteNonQuery();

            transaction.Commit();
        }

        public IEnumerable<Note> GetPublicNotes()
        {
            var notes = new List<Note>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, CreatedAt, ModifiedAt, OpenedAt FROM Notes WHERE VaultId IS NULL ORDER BY ModifiedAt DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                notes.Add(new Note
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    ModifiedAt = DateTime.Parse(reader.GetString(4)),
                    OpenedAt = DateTime.Parse(reader.GetString(5)),
                    VaultId = null
                });
            }

            return notes;
        }

        public IEnumerable<Note> GetVaultNotes(string vaultId)
        {
            var notes = new List<Note>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Content, CreatedAt, ModifiedAt, OpenedAt FROM Notes WHERE VaultId = $vaultId ORDER BY ModifiedAt DESC";
            command.Parameters.AddWithValue("$vaultId", vaultId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                notes.Add(new Note
                {
                    Id = reader.GetString(0),
                    Title = _encryptionService.Decrypt(vaultId, reader.GetString(1)),
                    Content = _encryptionService.Decrypt(vaultId, reader.GetString(2)),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    ModifiedAt = DateTime.Parse(reader.GetString(4)),
                    OpenedAt = DateTime.Parse(reader.GetString(5)),
                    VaultId = vaultId
                });
            }

            return notes;
        }

        public void SaveNote(Note note)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            string titleToSave = note.VaultId == null ? note.Title : _encryptionService.Encrypt(note.VaultId, note.Title);
            string contentToSave = note.VaultId == null ? note.Content : _encryptionService.Encrypt(note.VaultId, note.Content);

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Notes (Id, Title, Content, CreatedAt, ModifiedAt, OpenedAt, VaultId)
                VALUES ($id, $title, $content, $createdAt, $modifiedAt, $openedAt, $vaultId)
                ON CONFLICT(Id) DO UPDATE SET
                    Title = excluded.Title,
                    Content = excluded.Content,
                    ModifiedAt = excluded.ModifiedAt,
                    OpenedAt = excluded.OpenedAt,
                    VaultId = excluded.VaultId";

            command.Parameters.AddWithValue("$id", note.Id);
            command.Parameters.AddWithValue("$title", titleToSave);
            command.Parameters.AddWithValue("$content", contentToSave);
            command.Parameters.AddWithValue("$createdAt", note.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("$modifiedAt", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("$openedAt", note.OpenedAt.ToString("o"));
            command.Parameters.AddWithValue("$vaultId", note.VaultId ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }

        public void DeleteNote(string id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Notes WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
}