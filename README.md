# FolderGuard - C# Console Application for Folder Encryption

FolderGuard is a simple C# console application that allows you to encrypt and decrypt folders using AES encryption with key management.

## Features

- Encrypt a folder and generate a `.locked` file.
- Decrypt a `.locked` file and restore the original folder structure.

## Prerequisites

Before using FolderGuard, ensure you have the following:

- [.NET SDK](https://dotnet.microsoft.com/download/dotnet) installed on your machine.
- A `Key.key` file containing the encryption key in the application directory.
  - `Key.key` can be anything. You can rename a picture, an .exe, a .zip file or anything and use as a key.

## Usage

### Encrypt a Folder

To encrypt a folder, drag and drop the folder onto the `FolderGuard.exe` executable:
```bash
FolderGuard.exe "Path\To\Your\Folder"
```

The encrypted folder will be created with a .locked extension in the same directory as the original folder.

Decrypt a Folder

To decrypt a folder, drag and drop the .locked file onto the FolderGuard.exe executable:
```bash
FolderGuard.exe "Path\To\Your\Folder.locked"
```

The original folder will be restored in the same directory as the .locked file.

## Note
- If the folder you are trying to encrypt is empty, FolderGuard will display an error message and not perform the encryption.
- If the Key.key file is missing, FolderGuard will display an error message.

## Customization
You can customize the key file name by changing the `keyFile` variable in the `Program.cs` file.

