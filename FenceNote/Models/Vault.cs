using System;
using FenceNote.MVVM;

namespace FenceNote.Models
{
    public class Vault : ObservableObject
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = "New Vault";
        private bool _isEditing;
        private bool _isUnlocked;
        private bool _isSelected;
        private bool _isUnlockTarget;
        private int _lockoutSeconds = 60;
        private int _remainingLockoutSeconds;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Salt { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set => SetProperty(ref _isUnlocked, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsUnlockTarget
        {
            get => _isUnlockTarget;
            set => SetProperty(ref _isUnlockTarget, value);
        }

        public int LockoutSeconds
        {
            get => _lockoutSeconds;
            set
            {
                if (SetProperty(ref _lockoutSeconds, value))
                {
                    OnPropertyChanged(nameof(LockoutDisplay));
                }
            }
        }

        public int RemainingLockoutSeconds
        {
            get => _remainingLockoutSeconds;
            set
            {
                if (SetProperty(ref _remainingLockoutSeconds, value))
                {
                    OnPropertyChanged(nameof(RemainingLockoutDisplay));
                }
            }
        }

        public string LockoutDisplay
        {
            get
            {
                if (_lockoutSeconds < 60) return $"{_lockoutSeconds} sec";
                if (_lockoutSeconds == 60) return "1 min";
                return $"{_lockoutSeconds / 60} min";
            }
        }

        public string RemainingLockoutDisplay
        {
            get
            {
                TimeSpan t = TimeSpan.FromSeconds(_remainingLockoutSeconds);
                return t.ToString(@"m\:ss");
            }
        }
    }
}