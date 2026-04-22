using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace FenceNote.Services
{
    public class EncryptionService
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Iterations = 600_000;

        private readonly ConcurrentDictionary<string, byte[]> _vaultKeys = new();

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private string GenerateVerificationHash(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(KeySize);
            return Convert.ToBase64String(hash);
        }

        public (string Salt, string Hash) CreateVaultCredentials(string vaultId, string password)
        {
            byte[] salt = GenerateSalt();
            string hash = GenerateVerificationHash(password, salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            _vaultKeys[vaultId] = pbkdf2.GetBytes(KeySize);

            return (Convert.ToBase64String(salt), hash);
        }

        public bool UnlockVault(string vaultId, string password, string saltBase64, string expectedHash)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            string computedHash = GenerateVerificationHash(password, salt);

            if (computedHash == expectedHash)
            {
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                _vaultKeys[vaultId] = pbkdf2.GetBytes(KeySize);
                return true;
            }
            return false;
        }

        public void LockVault(string vaultId)
        {
            if (_vaultKeys.TryRemove(vaultId, out byte[]? key))
            {
                Array.Clear(key, 0, key.Length);
            }
        }

        public void LockAllVaults()
        {
            foreach (var key in _vaultKeys.Values)
            {
                Array.Clear(key, 0, key.Length);
            }
            _vaultKeys.Clear();
        }

        public bool IsVaultUnlocked(string vaultId)
        {
            return _vaultKeys.ContainsKey(vaultId);
        }

        public string Encrypt(string vaultId, string plainText)
        {
            if (!_vaultKeys.TryGetValue(vaultId, out byte[]? key))
                throw new InvalidOperationException("Vault is locked or does not exist.");

            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[plainBytes.Length];

            using (var aesGcm = new AesGcm(key))
            {
                aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
            }

            byte[] result = new byte[NonceSize + TagSize + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string vaultId, string cipherText)
        {
            if (!_vaultKeys.TryGetValue(vaultId, out byte[]? key))
                throw new InvalidOperationException("Vault is locked or does not exist.");

            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] fullCipher;
            try
            {
                fullCipher = Convert.FromBase64String(cipherText);
            }
            catch
            {
                return cipherText;
            }

            if (fullCipher.Length < NonceSize + TagSize)
            {
                return cipherText;
            }

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] cipherBytes = new byte[fullCipher.Length - NonceSize - TagSize];

            Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(fullCipher, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(fullCipher, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

            byte[] plainBytes = new byte[cipherBytes.Length];

            try
            {
                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
                }
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                return "--- ENCRYPTED CONTENT (DECRYPTION FAILED) ---";
            }
        }
    }
}