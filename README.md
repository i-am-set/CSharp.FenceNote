# FenceNote - A Secure C# Note-Taking Application

FenceNote is a secure, offline note-taking application built with C# designed to protect sensitive information. Developed with privacy in mind, FenceNote aims to provide an intuitive interface and robust encryption to meet the needs of everyone, from corporate professionals to everyday users wanting to keep their data private.

## Features

### Note Management
- **Create a Note**: Create standard public notes easily with just a few clicks.
- **Edit and Auto-Save**: Continuously edit notes with a smart auto-save system that updates the database the moment you pause typing.
- **Rename Note**: Change note titles to better organize your information.
- **Delete Note**: Quickly delete notes when you no longer need them.
- **Move to Vault**: Transfer public notes into secure vaults to encrypt them.

### Vault Management
- **Create a Vault**: Organize sensitive notes into secure, password-protected folders.
- **Lock and Unlock**: Manually lock vaults or unlock them using your secure password.
- **Auto-Lock Timer**: Set custom inactivity timers (from 15 seconds to 15 minutes) to automatically lock vaults when you step away.
- **Rename Vault**: Change the name of your secure vaults.
- **Delete Vault**: Remove vaults and all their encrypted contents.

### Security and Privacy
- **Automatic Encryption**: Automatically encrypt note titles and content using AES-GCM authenticated encryption when saved inside a vault.
- **Secure Password Protection**: Protect vaults using PBKDF2 password hashing with 600,000 iterations.
- **Memory Wiping**: Securely erase unlock keys from the computer's RAM when a vault is locked.
- **Offline Storage**: Store all data locally in an SQLite database without relying on cloud services.

### User Interface and Navigation
- **Dark Mode**: Switch between light and dark themes for comfortable viewing.
- **Right-Click Menus**: Access hidden menus to quickly lock vaults, change timers, or move notes.
- **Keyboard Shortcuts**: Use shortcuts like Ctrl+T for new notes, Ctrl+Shift+V for new vaults, or Ctrl+W to delete notes for faster navigation.

## Getting Started

> FenceNote is currently a Windows only product. FenceNote will only run on Windows 10 or higher.

### From Release

1. **Download the Latest Release**:
   - Navigate to the Releases page on the GitHub repository.

2. **Extract Files**:
   - After downloading the `.zip` file, extract it to a directory of your choice.

3. **Run the Application**:
   - Locate the extracted folder and double-click on `FenceNote.exe` to launch the application.

### Building From Source Code

> **Note**: Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed before proceeding. The application requires .NET 6.0 or a compatible version.

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/FenceNote.git
   ```
2. **Move Directory**:
   ```bash
   cd FenceNote
   ```
3. **Build Code**:
   ```bash
   dotnet build -c Release
   ```
4. **Move To Build Result**:
   ```bash
   cd bin\Release\net6.0-windows
   ```
5. **Run App**:
   ```bash
   "FenceNote.exe"
   ```
