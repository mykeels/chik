using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace Chik.Exams;

/// <summary>
/// Represents an IOC-friendly interface for file storage operations.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Creates a directory at the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder to create.</param>
    void CreateDirectory(string folderPath);

    /// <summary>
    /// Gets the folder path for the specified file path.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The folder path of the file.</returns>
    string GetFolderPath(string path);

    /// <summary>
    /// Gets the file path for the specified path.
    /// </summary>
    /// <param name="path">The path of the file or folder.</param>
    /// <returns>The file path.</returns>
    string GetFilePath(string path);

    /// <summary>
    /// Saves a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path where the file should be saved.</param>
    /// <param name="fileStream">The stream containing the file data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveFileAsync(string filePath, Stream fileStream);

    /// <summary>
    /// Opens a file stream for a file.
    /// </summary>
    /// <param name="filePath">The path where the file should be saved.</param>
    Stream CreateStream(string filePath);

    /// <summary>
    /// Zips a folder asynchronously.
    /// </summary>
    /// <param name="folderPath">The path of the folder to zip.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the path of the created zip file.</returns>
    Task<string> ZipFolderAsync(string folderPath);

    /// <summary>
    /// Zips a folder asynchronously, with a password.
    /// </summary>
    /// <param name="folderPath">The path of the folder to zip.</param>
    /// <param name="password">The password to use for the zip file.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the path of the created zip file.</returns>
    Task<string> ZipFolderAsync(string folderPath, string password);

    /// <summary>
    /// Unzips a zip file asynchronously.
    /// </summary>
    /// <param name="zipFilePath">The path of the zip file to unzip.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the path of the unzipped folder.</returns>
    Task<string> UnzipAsync(string zipFilePath);

    /// <summary>
    /// Unzips a zip file asynchronously, with a password.
    /// </summary>
    /// <param name="zipFilePath">The path of the zip file to unzip.</param>
    /// <param name="password">The password to use for the zip file.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the path of the unzipped folder.</returns>
    Task<string> UnzipAsync(string zipFilePath, string password);

    /// <summary>
    /// Deletes a folder asynchronously.
    /// </summary>
    /// <param name="folderPath">The path of the folder to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFolderAsync(string folderPath);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Checks if a file or directory exists asynchronously.
    /// </summary>
    /// <param name="path">The path of the file or directory to check.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result indicates whether the file or directory exists.
    /// Returns <c>false</c> if the path is invalid or inaccessible (e.g., due to insufficient permissions).
    /// </returns>
    Task<bool> ExistsAsync(string path);

    /// <summary>
    /// Checks if a file exists asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns>A task representing the asynchronous operation. The task result indicates whether the file exists or not.</returns>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Checks if a folder exists asynchronously.
    /// </summary>
    /// <param name="folderPath">The path of the folder to check.</param>
    /// <returns>A task representing the asynchronous operation. The task result indicates whether the folder exists or not.</returns>
    Task<bool> DirectoryExistsAsync(string folderPath);

    /// <summary>
    /// Reads the text content of a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to read.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the text content of the file.</returns>
    Task<string> ReadFileTextAsync(string filePath);

    /// <summary>
    /// Reads the content of a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to read.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the text content of the file.</returns>
    Task<Stream> ReadFileAsync(string filePath);

    /// <summary>
    /// Asynchronously retrieves a list of files from the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder to retrieve files from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of file paths.</returns>
    Task<List<FileInfo>> GetFilesAsync(string folderPath);

    /// <summary>
    /// Retrieves a list of folders asynchronously from the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder to retrieve folders from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of folder names.</returns>
    Task<List<DirectoryInfo>> GetFoldersAsync(string folderPath);

    /// <summary>
    /// Retrieves the list of files and folders within the specified folder asynchronously.
    /// </summary>
    /// <param name="folderPath">The path of the folder to retrieve files and folders from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of strings representing the files and folders.</returns>
    Task<List<FileSystemInfo>> GetEntriesAsync(string folderPath);

    /// <summary>
    /// Retrieves information about a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="FileInfo"/> of the file.</returns>
    Task<FileInfo> GetFileInfoAsync(string filePath);

    /// <summary>
    /// Asynchronously gets the directory information for the specified folder path.
    /// </summary>
    /// <param name="folderPath">The path of the folder.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the directory information.</returns>
    Task<DirectoryInfo> GetDirectoryInfoAsync(string folderPath);
}