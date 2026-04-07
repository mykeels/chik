using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace Chik.Exams;

public class FileStorage : IFileStorage
{
    private string _rootPath;

    public FileStorage(string rootPath)
    {
        _rootPath = rootPath;
    }

    public string GetFolderPath(string path)
    {
        return Path.Combine(
            _rootPath,
            Path.GetExtension(path) == "" ? path : Path.GetDirectoryName(path)!
        );
    }

    public string GetFilePath(string path)
    {
        return Path.Combine(_rootPath, path);
    }

    public void CreateDirectory(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        Directory.CreateDirectory(folderPath);
    }

    public async Task SaveFileAsync(string filePath, Stream fileStream)
    {
        filePath = Path.Combine(_rootPath, filePath);
        Console.WriteLine($"Saving file to {filePath}");
        string? fileDirPath = Path.GetDirectoryName(filePath);
        if (fileDirPath != null)
            Directory.CreateDirectory(fileDirPath);
        using (var file = File.Create(filePath))
        {
            await fileStream.CopyToAsync(file);
        }
    }

    public Stream CreateStream(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }
        return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public async Task<string> ZipFolderAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        string zipFilePath = $"{folderPath}.zip";
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }
        await Task.Run(
            () =>
                System.IO.Compression.ZipFile.CreateFromDirectory(
                    folderPath,
                    zipFilePath,
                    CompressionLevel.Fastest,
                    false
                )
        );
        return zipFilePath;
    }

    public async Task<string> ZipFolderAsync(string folderPath, string password)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        string zipFilePath = $"{folderPath}.zip";
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        await Task.Run(() =>
        {
            using (var zipStream = new ZipOutputStream(File.Create(zipFilePath)))
            {
                zipStream.SetLevel(9); // Maximum compression
                zipStream.Password = password;

                // Add all files in the directory
                foreach (
                    string file in Directory.GetFiles(
                        folderPath,
                        "*.*",
                        SearchOption.AllDirectories
                    )
                )
                {
                    string relativePath = file.Substring(folderPath.Length + 1);
                    var entry = new ZipEntry(relativePath);
                    zipStream.PutNextEntry(entry);

                    using (var fileStream = File.OpenRead(file))
                    {
                        fileStream.CopyTo(zipStream);
                    }
                }
            }
        });

        return zipFilePath;
    }

    public async Task<string> UnzipAsync(string zipFilePath)
    {
        zipFilePath = Path.Combine(_rootPath, zipFilePath);
        string? unzipFolderPath = Path.GetDirectoryName(zipFilePath);
        if (unzipFolderPath is null)
            throw new InvalidOperationException("Unzip folder path is null");
        unzipFolderPath = Path.Combine(
            unzipFolderPath,
            Path.GetFileNameWithoutExtension(zipFilePath)
        );
        if (Directory.Exists(unzipFolderPath))
            Directory.Delete(unzipFolderPath, true);
        await Task.Run(
            () => System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, unzipFolderPath)
        );
        return unzipFolderPath;
    }

    public async Task<string> UnzipAsync(string zipFilePath, string password)
    {
        zipFilePath = Path.Combine(_rootPath, zipFilePath);
        string? unzipFolderPath = Path.GetDirectoryName(zipFilePath);
        if (unzipFolderPath is null)
            throw new Exception("Unzip folder path is null");
        unzipFolderPath = Path.Combine(
            unzipFolderPath,
            Path.GetFileNameWithoutExtension(zipFilePath)
        );
        if (Directory.Exists(unzipFolderPath))
            Directory.Delete(unzipFolderPath, true);
        await Task.Run(() =>
        {
            using (var zipStream = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                zipStream.Password = password;
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory)
                        continue;
                    string entryPath = Path.Combine(unzipFolderPath, entry.Name);
                    string? entryDirPath = Path.GetDirectoryName(entryPath);
                    if (entryDirPath != null)
                        Directory.CreateDirectory(entryDirPath);
                    using (var fileStream = File.Create(entryPath))
                    {
                        byte[] buffer = new byte[4096];
                        int size;
                        while ((size = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, size);
                        }
                    }
                }
            }
        });
        return unzipFolderPath;
    }

    public async Task DeleteFolderAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        if (Directory.Exists(folderPath))
        {
            await Task.Run(() => Directory.Delete(folderPath, true));
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    public async Task<bool> ExistsAsync(string path)
    {
        path = Path.Combine(_rootPath, path);
        return await Task.FromResult(File.Exists(path) || Directory.Exists(path));
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        return await Task.FromResult(File.Exists(filePath));
    }

    public async Task<bool> DirectoryExistsAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        return await Task.FromResult(Directory.Exists(folderPath));
    }

    public async Task<string> ReadFileTextAsync(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        return await Task.FromResult(File.ReadAllText(filePath));
    }

    public async Task<Stream> ReadFileAsync(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        return await Task.FromResult(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
    }

    public async Task<List<FileInfo>> GetFilesAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        return await Task.FromResult(new DirectoryInfo(folderPath).GetFiles().ToList());
    }

    public async Task<List<DirectoryInfo>> GetFoldersAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        return await Task.FromResult(new DirectoryInfo(folderPath).GetDirectories().ToList());
    }

    public async Task<List<FileSystemInfo>> GetEntriesAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        return await Task.FromResult(new DirectoryInfo(folderPath).GetFileSystemInfos().ToList());
    }

    public async Task<FileInfo> GetFileInfoAsync(string filePath)
    {
        filePath = Path.Combine(_rootPath, filePath);
        return await Task.FromResult(new FileInfo(filePath));
    }

    public async Task<DirectoryInfo> GetDirectoryInfoAsync(string folderPath)
    {
        folderPath = Path.Combine(_rootPath, folderPath);
        return await Task.FromResult(new DirectoryInfo(folderPath));
    }
}