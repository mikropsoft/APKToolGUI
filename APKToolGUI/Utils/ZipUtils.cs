using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace APKToolGUI.Utils
{
    public class ZipUtils
    {
        public static string GetFileName(string path, string fileNameContains, string folderContains = "")
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                var entry = archive.Entries
                    .FirstOrDefault(e => e.FullName.Contains(fileNameContains) &&
                                         (string.IsNullOrEmpty(folderContains) || e.FullName.Contains(folderContains)));
                return entry != null ? Path.GetFileName(entry.FullName) : string.Empty;
            }
        }

        public static string GetFileNameWithoutExtension(string path, string fileNameContains, string folderContains = "")
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                var entry = archive.Entries
                    .FirstOrDefault(e => e.FullName.Contains(fileNameContains) &&
                                         (string.IsNullOrEmpty(folderContains) || e.FullName.Contains(folderContains)));
                return entry != null ? Path.GetFileNameWithoutExtension(entry.FullName) : string.Empty;
            }
        }

        public static bool Exists(string path, string fileNameContains, string folderContains = "")
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                return archive.Entries.Any(e => e.FullName.Contains(fileNameContains) &&
                                                (string.IsNullOrEmpty(folderContains) || e.FullName.Contains(folderContains)));
            }
        }

        public static void AddFile(string zipPath, string filePath, string targetFolderInZip = "")
        {
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    // Combine the target folder with the file name to create the entry path
                    string fileName = Path.GetFileName(filePath);
                    string entryPath = string.IsNullOrEmpty(targetFolderInZip)
                        ? fileName
                        : Path.Combine(targetFolderInZip, fileName).Replace("\\", "/");

                    // Remove the entry if it already exists
                    var existingEntry = archive.GetEntry(entryPath);
                    existingEntry?.Delete();

                    // Add the file to the archive
                    archive.CreateEntryFromFile(filePath, entryPath, CompressionLevel.Optimal);

                    Console.WriteLine($"Added '{filePath}' to ZIP archive at '{entryPath}'.");
                }
            }
        }

        public static void RemoveFile(string zipFile, string fileName)
        {
            using (FileStream fs = new FileStream(zipFile, FileMode.OpenOrCreate))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                var entry = archive.Entries.FirstOrDefault(e => e.FullName.Contains(fileName));
                entry?.Delete();
            }
        }

        public static void ExtractFile(string path, string fileName, string destination)
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                var entry = archive.Entries.FirstOrDefault(e => e.FullName.Contains(fileName));
                entry?.ExtractToFile(Path.Combine(destination, Path.GetFileName(entry.FullName)), true);
            }
        }

        public static void ExtractAll(string path, string destination, bool flattenFoldersOnExtract = false)
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (var entry in archive.Entries)
                {
                    string fullPath = flattenFoldersOnExtract
                        ? Path.Combine(destination, Path.GetFileName(entry.FullName))
                        : Path.Combine(destination, entry.FullName);
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directoryPath)) Directory.CreateDirectory(directoryPath);
                    entry.ExtractToFile(fullPath, true);
                }
            }
        }

        public static void AddDirectory(string zipPath, string directoryPath, string directoryPathInArchive = "")
        {
            if (!File.Exists(zipPath))
            {
                Console.WriteLine("ZIP file does not exist.");
                return;
            }

            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                    {
                        // Calculate the relative path and prepend the target folder inside the ZIP
                        string relativePath = GetRelativePath(directoryPath, filePath);
                        string entryPath = Path.Combine(directoryPathInArchive, relativePath).Replace("\\", "/");

                        // Remove the entry if it already exists
                        var existingEntry = archive.GetEntry(entryPath);
                        existingEntry?.Delete();

                        // Add the file to the archive
                        archive.CreateEntryFromFile(filePath, entryPath, CompressionLevel.Optimal);
                    }
                }
            }
        }

        static string GetRelativePath(string basePath, string fullPath)
        {
            // Ensure both paths are absolute
            basePath = Path.GetFullPath(basePath);
            fullPath = Path.GetFullPath(fullPath);

            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The fullPath is not within the basePath.");
            }

            return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static void ExtractDirectory(string path, string folderName, string destination, bool flattenFoldersOnExtract = false)
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries.Where(e => e.FullName.Contains(folderName)))
                {
                    string extractPath = flattenFoldersOnExtract
                        ? Path.Combine(destination, Path.GetFileName(entry.FullName))
                        : Path.Combine(destination, entry.FullName);

                    string directoryPath = Path.GetDirectoryName(extractPath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    entry.ExtractToFile(extractPath, true);
                }
            }
        }
    }
}
