using System;
using FenceNote.MVVM;

namespace FenceNote.Models
{
    public class Note : ObservableObject
    {
        private string _id = Guid.NewGuid().ToString();
        private string _title = "New Note";
        private string _content = string.Empty;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _modifiedAt = DateTime.UtcNow;
        private DateTime _openedAt = DateTime.UtcNow;
        private string? _vaultId;
        private bool _isEditing;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public DateTime ModifiedAt
        {
            get => _modifiedAt;
            set => SetProperty(ref _modifiedAt, value);
        }

        public DateTime OpenedAt
        {
            get => _openedAt;
            set => SetProperty(ref _openedAt, value);
        }

        public string? VaultId
        {
            get => _vaultId;
            set => SetProperty(ref _vaultId, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
    }
}