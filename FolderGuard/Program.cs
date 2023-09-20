using System;
using System.IO;

class Program
{
  static void Main(string[] args)
  {
    if (args.Length == 0)
    {
      Console.WriteLine("Usage: Drag and drop a folder or a .locked file onto this exe.");
      return;
    }

    string inputFile = args[0];
    string keyFile = "Key.key";

    if (inputFile.EndsWith(".locked", StringComparison.OrdinalIgnoreCase))
    {
      // Unlock the folder
      string outputFolder = inputFile.Replace(".locked", "_unlocked");
      DecryptFolder(inputFile, keyFile, outputFolder);
    }
    else if (Directory.Exists(inputFile))
    {
      // Lock the folder
      string lockedFile = inputFile + ".locked";
      EncryptFolder(inputFile, keyFile, lockedFile);
    }
    else
    {
      Console.WriteLine("Invalid input. Drag and drop a folder or a .locked file.");
    }
  }

  static void EncryptFolder(string inputFolder, string keyFile, string outputFile)
  {
    Console.WriteLine($"Encrypting {inputFolder} to {outputFile} with key {keyFile}");
  }

  static void DecryptFolder(string inputFile, string keyFile, string outputFolder)
  {
    Console.WriteLine($"Decrypting {inputFile} to {outputFolder} with key {keyFile}");
  }
}
