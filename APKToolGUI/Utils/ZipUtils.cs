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

        public static void AddFile(string zipFile, string fileName, string directoryPathInArchive = "")
        {
            using (FileStream fs = new FileStream(zipFile, FileMode.OpenOrCreate))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                string entryName = string.IsNullOrEmpty(directoryPathInArchive) ? fileName : $"{directoryPathInArchive}/{Path.GetFileName(fileName)}";
                archive.CreateEntryFromFile(fileName, entryName);
            }
        }

        public static void UpdateFile(string zipFile, string fileName, string directoryPathInArchive = "")
        {
            RemoveFile(zipFile, fileName);
            AddFile(zipFile, fileName, directoryPathInArchive);
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

        public static void AddDirectory(string path, string directoryPath, string directoryPathInArchive = "")
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    string entryName = string.IsNullOrEmpty(directoryPathInArchive)
                        ? filePath.Substring(directoryPath.Length + 1)
                        : Path.Combine(directoryPathInArchive, filePath.Substring(directoryPath.Length + 1));
                    archive.CreateEntryFromFile(filePath, entryName.Replace("\\", "/"));
                }
            }
        }

        public static void UpdateDirectory(string path, string directoryPath, string directoryPathInArchive = "")
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Update))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    string entryName = string.IsNullOrEmpty(directoryPathInArchive)
                        ? filePath.Substring(directoryPath.Length + 1)
                        : Path.Combine(directoryPathInArchive, filePath.Substring(directoryPath.Length + 1));
                    RemoveFile(path, entryName.Replace("\\", "/"));
                    archive.CreateEntryFromFile(filePath, entryName.Replace("\\", "/"));
                }
            }
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
