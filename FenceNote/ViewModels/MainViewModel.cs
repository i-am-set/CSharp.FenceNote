using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FenceNote.Models;
using FenceNote.MVVM;
using FenceNote.Services;

namespace FenceNote.ViewModels
{
    public class NotificationItem
    {
        public string Message { get; set; } = string.Empty;
    }

    public class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly SettingsService _settingsService;
        private readonly EncryptionService _encryptionService;
        private readonly DispatcherTimer _activityTimer;
        private readonly DispatcherTimer _autoSaveTimer;
        private DateTime _lastActivity = DateTime.Now;

        private Note? _selectedNote;
        private Vault? _selectedVault;
        private bool _isDeletePromptOpen;
        private Note? _noteToDelete;

        private bool _isVaultPromptOpen;
        private bool _isMoveToVaultPromptOpen;
        private bool _isUnlockPromptOpen;

        private string _vaultNameInput = string.Empty;
        private string _vaultPasswordInput = string.Empty;
        private string _vaultConfirmPasswordInput = string.Empty;
        private string _unlockPasswordInput = string.Empty;

        private Vault? _vaultToUnlock;
        private Note? _noteToMove;
        private Action? _pendingUnlockAction;

        public AppSettings Settings { get; private set; }

        public ObservableCollection<Vault> Vaults { get; }
        public ObservableCollection<Note> DisplayedNotes { get; }
        public ObservableCollection<NotificationItem> Notifications { get; } = new();

        public event EventHandler<bool>? DarkModeRequested;

        public Note? SelectedNote
        {
            get => _selectedNote;
            set
            {
                if (_selectedNote != null)
                {
                    _selectedNote.PropertyChanged -= SelectedNote_PropertyChanged;
                }

                if (SetProperty(ref _selectedNote, value))
                {
                    if (_selectedNote != null)
                    {
                        _selectedNote.PropertyChanged += SelectedNote_PropertyChanged;
                    }

                    IsDeletePromptOpen = false;
                    OnPropertyChanged(nameof(IsNoteSelected));
                }
            }
        }

        public Vault? SelectedVault
        {
            get => _selectedVault;
            set
            {
                var oldVault = _selectedVault;
                if (SetProperty(ref _selectedVault, value))
                {
                    if (oldVault != null)
                    {
                        oldVault.IsSelected = false;
                    }

                    if (_selectedVault != null)
                    {
                        _selectedVault.IsSelected = true;
                    }

                    LoadNotesForSelection();
                    OnPropertyChanged(nameof(IsVaultSelected));
                    OnPropertyChanged(nameof(IsPublicNotesSelected));
                    OnPropertyChanged(nameof(NotesPaneTitle));
                }
            }
        }

        public Vault? VaultToUnlock
        {
            get => _vaultToUnlock;
            set
            {
                if (SetProperty(ref _vaultToUnlock, value))
                {
                    OnPropertyChanged(nameof(UnlockPromptTitle));
                }
            }
        }

        public bool IsNoteSelected => SelectedNote != null;
        public bool IsVaultSelected => SelectedVault != null;
        public bool IsPublicNotesSelected => SelectedVault == null;
        public string NotesPaneTitle => SelectedVault == null ? "PUBLIC NOTES" : $"{SelectedVault.Name.ToUpper()} NOTES";
        public string UnlockPromptTitle => VaultToUnlock != null ? $"Unlock Vault \"{VaultToUnlock.Name}\"" : "Unlock Vault";

        public bool IsDeletePromptOpen
        {
            get => _isDeletePromptOpen;
            set => SetProperty(ref _isDeletePromptOpen, value);
        }

        public bool IsVaultPromptOpen
        {
            get => _isVaultPromptOpen;
            set
            {
                if (SetProperty(ref _isVaultPromptOpen, value) && !value)
                {
                    VaultNameInput = string.Empty;
                    VaultPasswordInput = string.Empty;
                    VaultConfirmPasswordInput = string.Empty;
                }
            }
        }

        public bool IsMoveToVaultPromptOpen
        {
            get => _isMoveToVaultPromptOpen;
            set => SetProperty(ref _isMoveToVaultPromptOpen, value);
        }

        public bool IsUnlockPromptOpen
        {
            get => _isUnlockPromptOpen;
            set
            {
                if (SetProperty(ref _isUnlockPromptOpen, value))
                {
                    if (!value)
                    {
                        UnlockPasswordInput = string.Empty;
                        if (VaultToUnlock != null)
                        {
                            VaultToUnlock.IsUnlockTarget = false;
                            VaultToUnlock = null;
                        }
                    }
                    else if (VaultToUnlock != null)
                    {
                        VaultToUnlock.IsUnlockTarget = true;
                    }
                }
            }
        }

        public string VaultNameInput
        {
            get => _vaultNameInput;
            set => SetProperty(ref _vaultNameInput, value);
        }

        public string VaultPasswordInput
        {
            get => _vaultPasswordInput;
            set => SetProperty(ref _vaultPasswordInput, value);
        }

        public string VaultConfirmPasswordInput
        {
            get => _vaultConfirmPasswordInput;
            set => SetProperty(ref _vaultConfirmPasswordInput, value);
        }

        public string UnlockPasswordInput
        {
            get => _unlockPasswordInput;
            set => SetProperty(ref _unlockPasswordInput, value);
        }

        public ICommand AddNoteCommand { get; }
        public ICommand RequestDeleteNoteCommand { get; }
        public ICommand ConfirmDeleteNoteCommand { get; }
        public ICommand CancelDeleteNoteCommand { get; }
        public ICommand BeginRenameNoteCommand { get; }

        public ICommand RequestCreateVaultCommand { get; }
        public ICommand ConfirmCreateVaultCommand { get; }
        public ICommand CancelCreateVaultCommand { get; }
        public ICommand DeleteVaultCommand { get; }
        public ICommand ClearVaultSelectionCommand { get; }
        public ICommand SelectVaultCommand { get; }
        public ICommand BeginRenameVaultCommand { get; }
        public ICommand LockVaultCommand { get; }

        public ICommand ConfirmUnlockVaultCommand { get; }
        public ICommand CancelUnlockVaultCommand { get; }

        public ICommand RequestMoveToVaultCommand { get; }
        public ICommand ConfirmMoveToVaultCommand { get; }
        public ICommand CancelMoveToVaultCommand { get; }

        public ICommand SetLockout15sCommand { get; }
        public ICommand SetLockout30sCommand { get; }
        public ICommand SetLockout1mCommand { get; }
        public ICommand SetLockout5mCommand { get; }
        public ICommand SetLockout15mCommand { get; }

        public ICommand ToggleDarkModeCommand { get; }

        public MainViewModel()
        {
            _encryptionService = new EncryptionService();
            _databaseService = new DatabaseService(_encryptionService);
            _settingsService = new SettingsService();
            Settings = _settingsService.LoadSettings();

            Vaults = new ObservableCollection<Vault>(_databaseService.GetAllVaults());
            DisplayedNotes = new ObservableCollection<Note>();

            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _activityTimer.Tick += (s, e) => CheckVaultLockouts();

            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _autoSaveTimer.Tick += (s, e) =>
            {
                _autoSaveTimer.Stop();
                SaveCurrentNote();
            };

            AddNoteCommand = new RelayCommand(_ => AddNewNote());

            RequestDeleteNoteCommand = new RelayCommand(p =>
            {
                _noteToDelete = p as Note ?? SelectedNote;
                if (_noteToDelete != null) IsDeletePromptOpen = true;
            });
            ConfirmDeleteNoteCommand = new RelayCommand(_ => ExecuteDeleteNote());
            CancelDeleteNoteCommand = new RelayCommand(_ => IsDeletePromptOpen = false);

            BeginRenameNoteCommand = new RelayCommand(p =>
            {
                if (p is Note note) note.IsEditing = true;
            });

            RequestCreateVaultCommand = new RelayCommand(_ => IsVaultPromptOpen = true);
            ConfirmCreateVaultCommand = new RelayCommand(_ => ExecuteCreateVault());
            CancelCreateVaultCommand = new RelayCommand(_ => IsVaultPromptOpen = false);

            DeleteVaultCommand = new RelayCommand(p =>
            {
                if (p is Vault vault)
                {
                    if (!vault.IsUnlocked)
                    {
                        VaultToUnlock = vault;
                        _pendingUnlockAction = () => ExecuteDeleteVault(vault);
                        IsUnlockPromptOpen = true;
                    }
                    else
                    {
                        ExecuteDeleteVault(vault);
                    }
                }
            });

            ClearVaultSelectionCommand = new RelayCommand(_ => SelectedVault = null);

            SelectVaultCommand = new RelayCommand(p =>
            {
                if (p is Vault vault)
                {
                    if (!vault.IsUnlocked)
                    {
                        VaultToUnlock = vault;
                        _pendingUnlockAction = () => SelectedVault = vault;
                        IsUnlockPromptOpen = true;
                    }
                    else
                    {
                        SelectedVault = vault;
                    }
                }
            });

            BeginRenameVaultCommand = new RelayCommand(p =>
            {
                if (p is Vault vault)
                {
                    if (!vault.IsUnlocked)
                    {
                        VaultToUnlock = vault;
                        _pendingUnlockAction = () => vault.IsEditing = true;
                        IsUnlockPromptOpen = true;
                    }
                    else
                    {
                        vault.IsEditing = true;
                    }
                }
            });

            LockVaultCommand = new RelayCommand(p => ExecuteLockVault(p as Vault));

            ConfirmUnlockVaultCommand = new RelayCommand(_ => ExecuteUnlockVault());
            CancelUnlockVaultCommand = new RelayCommand(_ =>
            {
                IsUnlockPromptOpen = false;
                _pendingUnlockAction = null;
            });

            RequestMoveToVaultCommand = new RelayCommand(p =>
            {
                _noteToMove = p as Note ?? SelectedNote;
                if (_noteToMove != null && _noteToMove.VaultId == null)
                {
                    IsMoveToVaultPromptOpen = true;
                }
            });

            ConfirmMoveToVaultCommand = new RelayCommand(p =>
            {
                if (p is Vault targetVault)
                {
                    if (!targetVault.IsUnlocked)
                    {
                        VaultToUnlock = targetVault;
                        _pendingUnlockAction = () =>
                        {
                            ExecuteMoveToVault(targetVault);
                            ExecuteLockVault(targetVault);
                        };
                        IsUnlockPromptOpen = true;
                    }
                    else
                    {
                        ExecuteMoveToVault(targetVault);
                    }
                }
            });

            CancelMoveToVaultCommand = new RelayCommand(_ =>
            {
                IsMoveToVaultPromptOpen = false;
                _noteToMove = null;
            });

            SetLockout15sCommand = new RelayCommand(p => SetVaultLockout(p as Vault, 15));
            SetLockout30sCommand = new RelayCommand(p => SetVaultLockout(p as Vault, 30));
            SetLockout1mCommand = new RelayCommand(p => SetVaultLockout(p as Vault, 60));
            SetLockout5mCommand = new RelayCommand(p => SetVaultLockout(p as Vault, 300));
            SetLockout15mCommand = new RelayCommand(p => SetVaultLockout(p as Vault, 900));

            ToggleDarkModeCommand = new RelayCommand(_ =>
            {
                Settings.IsDarkMode = !Settings.IsDarkMode;
                _settingsService.SaveSettings(Settings);
                DarkModeRequested?.Invoke(this, Settings.IsDarkMode);
            });

            LoadNotesForSelection();
        }

        private void SelectedNote_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Note.Title) || e.PropertyName == nameof(Note.Content))
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Start();
            }
        }

        public void TriggerInitialTheme()
        {
            DarkModeRequested?.Invoke(this, Settings.IsDarkMode);
        }

        public void ResetIdleTimer()
        {
            _lastActivity = DateTime.Now;
        }

        private void CheckVaultLockouts()
        {
            bool hasUnlockedVaults = false;
            var idleSeconds = (DateTime.Now - _lastActivity).TotalSeconds;

            foreach (var vault in Vaults)
            {
                if (!vault.IsUnlocked) continue;

                hasUnlockedVaults = true;
                int remaining = vault.LockoutSeconds - (int)idleSeconds;

                if (remaining <= 0)
                {
                    ExecuteLockVault(vault);
                }
                else
                {
                    vault.RemainingLockoutSeconds = remaining;
                }
            }

            if (!hasUnlockedVaults)
            {
                _activityTimer.Stop();
            }
        }

        private void SetVaultLockout(Vault? vault, int seconds)
        {
            if (vault != null)
            {
                vault.LockoutSeconds = seconds;
                Task.Run(() => _databaseService.SaveVault(vault));
            }
        }

        private async void ShowNotification(string message)
        {
            var notification = new NotificationItem { Message = $"{message} at {DateTime.Now:h:mm:ss tt}" };
            Notifications.Add(notification);
            await Task.Delay(3000);
            Notifications.Remove(notification);
        }

        private string GetUniqueVaultName(string baseName, string? excludeId = null)
        {
            string name = string.IsNullOrWhiteSpace(baseName) ? "New Vault" : baseName.Trim();
            string finalName = name;
            int counter = 1;

            while (Vaults.Any(v => v.Id != excludeId && v.Name.Equals(finalName, StringComparison.OrdinalIgnoreCase)))
            {
                finalName = $"{name} {counter}";
                counter++;
            }
            return finalName;
        }

        private string GetUniqueNoteTitle(string baseTitle, string? excludeId = null)
        {
            string title = string.IsNullOrWhiteSpace(baseTitle) ? "New Note" : baseTitle.Trim();
            string finalTitle = title;
            int counter = 1;

            while (DisplayedNotes.Any(n => n.Id != excludeId && n.Title.Equals(finalTitle, StringComparison.OrdinalIgnoreCase)))
            {
                finalTitle = $"{title} {counter}";
                counter++;
            }
            return finalTitle;
        }

        private async void LoadNotesForSelection()
        {
            DisplayedNotes.Clear();
            var currentVaultId = _selectedVault?.Id;

            var notes = await Task.Run(() =>
            {
                if (currentVaultId == null)
                    return _databaseService.GetPublicNotes();
                else
                    return _databaseService.GetVaultNotes(currentVaultId);
            });

            if (_selectedVault?.Id != currentVaultId) return;

            foreach (var note in notes)
            {
                DisplayedNotes.Add(note);
            }
        }

        private void AddNewNote()
        {
            var newNote = new Note
            {
                Title = GetUniqueNoteTitle("New Note"),
                VaultId = SelectedVault?.Id,
                ModifiedAt = DateTime.UtcNow
            };

            DisplayedNotes.Insert(0, newNote);
            SelectedNote = newNote;

            var noteSnapshot = new Note
            {
                Id = newNote.Id,
                Title = newNote.Title,
                Content = newNote.Content,
                CreatedAt = newNote.CreatedAt,
                ModifiedAt = newNote.ModifiedAt,
                OpenedAt = newNote.OpenedAt,
                VaultId = newNote.VaultId
            };

            Task.Run(() => _databaseService.SaveNote(noteSnapshot));
        }

        private void SaveCurrentNote()
        {
            if (SelectedNote != null)
            {
                SelectedNote.ModifiedAt = DateTime.UtcNow;

                var noteSnapshot = new Note
                {
                    Id = SelectedNote.Id,
                    Title = SelectedNote.Title,
                    Content = SelectedNote.Content,
                    CreatedAt = SelectedNote.CreatedAt,
                    ModifiedAt = SelectedNote.ModifiedAt,
                    OpenedAt = SelectedNote.OpenedAt,
                    VaultId = SelectedNote.VaultId
                };

                Task.Run(() => _databaseService.SaveNote(noteSnapshot));
            }
        }

        public void ForceSavePendingChanges()
        {
            if (_autoSaveTimer.IsEnabled)
            {
                _autoSaveTimer.Stop();
                if (SelectedNote != null)
                {
                    SelectedNote.ModifiedAt = DateTime.UtcNow;
                    _databaseService.SaveNote(SelectedNote);
                }
            }
        }

        private void ExecuteDeleteNote()
        {
            if (_noteToDelete != null)
            {
                string id = _noteToDelete.Id;
                Task.Run(() => _databaseService.DeleteNote(id));

                DisplayedNotes.Remove(_noteToDelete);

                if (SelectedNote == _noteToDelete)
                {
                    SelectedNote = null;
                }

                _noteToDelete = null;
                IsDeletePromptOpen = false;
            }
        }

        private async void ExecuteCreateVault()
        {
            if (string.IsNullOrWhiteSpace(VaultPasswordInput) || VaultPasswordInput != VaultConfirmPasswordInput)
            {
                ShowNotification("Passwords do not match or are empty");
                return;
            }

            var newVault = new Vault
            {
                Name = GetUniqueVaultName(VaultNameInput)
            };

            string password = VaultPasswordInput;
            string vaultId = newVault.Id;

            var credentials = await Task.Run(() => _encryptionService.CreateVaultCredentials(vaultId, password));

            newVault.Salt = credentials.Salt;
            newVault.PasswordHash = credentials.Hash;
            newVault.IsUnlocked = true;
            newVault.RemainingLockoutSeconds = newVault.LockoutSeconds;

            Vaults.Add(newVault);
            await Task.Run(() => _databaseService.SaveVault(newVault));

            _activityTimer.Start();
            IsVaultPromptOpen = false;

            if (_isMoveToVaultPromptOpen && _noteToMove != null)
            {
                ExecuteMoveToVault(newVault);
            }
            else
            {
                SelectedVault = newVault;
            }
        }

        private async void ExecuteUnlockVault()
        {
            if (VaultToUnlock == null || string.IsNullOrEmpty(UnlockPasswordInput)) return;

            string password = UnlockPasswordInput;
            string vaultId = VaultToUnlock.Id;
            string salt = VaultToUnlock.Salt;
            string hash = VaultToUnlock.PasswordHash;

            bool isUnlocked = await Task.Run(() => _encryptionService.UnlockVault(vaultId, password, salt, hash));

            if (isUnlocked)
            {
                VaultToUnlock.IsUnlocked = true;
                VaultToUnlock.RemainingLockoutSeconds = VaultToUnlock.LockoutSeconds;

                _activityTimer.Start();

                _pendingUnlockAction?.Invoke();
                _pendingUnlockAction = null;

                IsUnlockPromptOpen = false;
            }
            else
            {
                ShowNotification("Invalid Password");
                UnlockPasswordInput = string.Empty;
            }
        }

        private void ExecuteLockVault(Vault? vault)
        {
            if (vault == null) return;

            _encryptionService.LockVault(vault.Id);
            vault.IsUnlocked = false;

            if (SelectedVault == vault)
            {
                SelectedVault = null;
            }
        }

        private void ExecuteDeleteVault(Vault? vault)
        {
            if (vault != null)
            {
                string id = vault.Id;
                Task.Run(() =>
                {
                    _databaseService.DeleteVault(id);
                    _encryptionService.LockVault(id);
                });

                Vaults.Remove(vault);

                if (SelectedVault == vault)
                {
                    SelectedVault = null;
                }
            }
        }

        private void ExecuteMoveToVault(Vault? targetVault)
        {
            if (targetVault == null || _noteToMove == null) return;

            _noteToMove.VaultId = targetVault.Id;

            var noteSnapshot = new Note
            {
                Id = _noteToMove.Id,
                Title = _noteToMove.Title,
                Content = _noteToMove.Content,
                CreatedAt = _noteToMove.CreatedAt,
                ModifiedAt = _noteToMove.ModifiedAt,
                OpenedAt = _noteToMove.OpenedAt,
                VaultId = _noteToMove.VaultId
            };

            Task.Run(() => _databaseService.SaveNote(noteSnapshot));

            if (SelectedVault == null)
            {
                DisplayedNotes.Remove(_noteToMove);
                if (SelectedNote == _noteToMove)
                {
                    SelectedNote = null;
                }
            }

            IsMoveToVaultPromptOpen = false;
            _noteToMove = null;
            ShowNotification($"Moved to {targetVault.Name}");
        }

        public void CommitVaultRename(Vault vault, string newName)
        {
            if (!vault.IsEditing) return;
            vault.Name = GetUniqueVaultName(newName, vault.Id);
            vault.IsEditing = false;
            Task.Run(() => _databaseService.SaveVault(vault));
        }

        public void CancelVaultRename(Vault vault)
        {
            vault.IsEditing = false;
        }

        public void CommitNoteRename(Note note, string newName)
        {
            if (!note.IsEditing) return;
            note.Title = GetUniqueNoteTitle(newName, note.Id);
            note.IsEditing = false;

            var noteSnapshot = new Note
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                ModifiedAt = note.ModifiedAt,
                OpenedAt = note.OpenedAt,
                VaultId = note.VaultId
            };

            Task.Run(() => _databaseService.SaveNote(noteSnapshot));
        }

        public void CancelNoteRename(Note note)
        {
            note.IsEditing = false;
        }
    }
}