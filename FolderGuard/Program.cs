using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

class Program
{
  static void Main(string[] args)
  {
    if (args.Length == 0)
    {
      PrintMessage("Usage: Drag and drop a folder or a .locked file onto this exe.");
      Console.ReadKey(true);
      return;
    }

    string keyFile = "Key.key";

    if (!File.Exists(keyFile))
    {
      PrintErrorMessage("Error: Key.key file not found.");
      Console.ReadKey(true);
      return;
    }

    foreach (string inputFile in args)
    {
      if (inputFile.EndsWith(".locked", StringComparison.OrdinalIgnoreCase))
      {
        // Unlock the folder
        string outputFolder = inputFile.Replace(".locked", "");
        try
        {
          DecryptFolder(inputFile, keyFile, outputFolder);
        }
        catch
        {
          PrintErrorMessage($"Failed to decrypt {inputFile}.");
        }
      }
      else if (Directory.Exists(inputFile))
      {
        // Lock the folder
        string lockedFile = inputFile + ".locked";
        try
        {
          EncryptFolder(inputFile, keyFile, lockedFile);
        }
        catch
        {
          PrintErrorMessage($"Failed to encrypt {inputFile}.");
        }
      }
      else
      {
        PrintErrorMessage($"Invalid input: {inputFile}");
      }
    }

    PrintSuccessMessage("Press any key to exit.");
    Console.ReadKey(true);
  }

  static void EncryptFolder(string inputFolder, string keyFile, string outputFile)
  {
    if (Directory.GetFiles(inputFolder, "*", SearchOption.AllDirectories).Length == 0)
    {
      PrintErrorMessage($"Error: {inputFolder} is an empty folder. Nothing to encrypt.");
      return;
    }

    PrintMessage($"Encrypting {inputFolder} to {outputFile} with key {keyFile}");

    using (Aes aesAlg = Aes.Create())
    {
      byte[] derivedKey = DeriveKeyFromFile(keyFile, aesAlg.KeySize / 8);
      byte[] iv = aesAlg.IV;

      using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create))
      {
        fsOutput.Write(iv, 0, iv.Length);
        using (CryptoStream csEncrypt = new CryptoStream(fsOutput, aesAlg.CreateEncryptor(derivedKey, iv), CryptoStreamMode.Write))
        {
          CompressAndEncryptDirectory(inputFolder, csEncrypt);
        }
      }
    }
  }

  static void DecryptFolder(string inputFile, string keyFile, string outputFolder)
  {
    PrintMessage($"Decrypting {inputFile} to {outputFolder} with key {keyFile}");

    using (Aes aesAlg = Aes.Create())
    {
      byte[] derivedKey = DeriveKeyFromFile(keyFile, aesAlg.KeySize / 8);

      using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
      {
        byte[] iv = new byte[aesAlg.IV.Length];
        fsInput.Read(iv, 0, iv.Length);

        using (CryptoStream csDecrypt = new CryptoStream(fsInput, aesAlg.CreateDecryptor(derivedKey, iv), CryptoStreamMode.Read))
        {
          DecryptAndDecompressDirectory(outputFolder, csDecrypt);
        }
      }
    }
  }

  static byte[] DeriveKeyFromFile(string keyFile, int keySizeInBytes)
  {
    // Derive a key from the key file using PBKDF2
    using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(File.ReadAllText(keyFile), new byte[0], 10000))
    {
      return keyDerivation.GetBytes(keySizeInBytes);
    }
  }

  static void CompressAndEncryptDirectory(string directory, CryptoStream cryptoStream)
  {
    using (ZipArchive zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Create, leaveOpen: true))
    {
      CompressDirectory(directory, zipArchive);
    }
  }

  static void DecryptAndDecompressDirectory(string directory, CryptoStream cryptoStream)
  {
    using (ZipArchive zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Read, leaveOpen: true))
    {
      DecompressDirectory(directory, zipArchive);
    }
  }

  static void CompressDirectory(string sourceDirectory, ZipArchive zipArchive)
  {
    string[] files = Directory.GetFiles(sourceDirectory);
    foreach (string file in files)
    {
      string entryName = Path.GetRelativePath(sourceDirectory, file);
      ZipArchiveEntry entry = zipArchive.CreateEntry(entryName, CompressionLevel.Optimal);
      using (Stream entryStream = entry.Open())
      using (FileStream fileStream = File.OpenRead(file))
      {
        fileStream.CopyTo(entryStream);
      }
    }

    string[] subdirectories = Directory.GetDirectories(sourceDirectory);
    foreach (string subdirectory in subdirectories)
    {
      string entryName = Path.GetRelativePath(sourceDirectory, subdirectory) + "/";
      zipArchive.CreateEntry(entryName);
      CompressDirectory(subdirectory, zipArchive);
    }
  }

  static void DecompressDirectory(string targetDirectory, ZipArchive zipArchive)
  {
    foreach (ZipArchiveEntry entry in zipArchive.Entries)
    {
      string entryPath = Path.Combine(targetDirectory, entry.FullName);
      if (entry.Name == string.Empty)
      {
        Directory.CreateDirectory(entryPath);
      }
      else
      {
        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
        using (Stream entryStream = entry.Open())
        using (FileStream fileStream = File.Create(entryPath))
        {
          entryStream.CopyTo(fileStream);
        }
      }
    }
  }

  // Custom Console.WriteLine methods for different message types
  static void PrintErrorMessage(string message)
  {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
  }

  static void PrintSuccessMessage(string message)
  {
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(message);
    Console.ResetColor();
  }

  static void PrintMessage(string message)
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(message);
    Console.ResetColor();
  }
}
