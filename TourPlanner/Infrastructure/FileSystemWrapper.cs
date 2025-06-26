using TourPlanner.Infrastructure.Interfaces;

namespace TourPlanner.Infrastructure;

public class FileSystemWrapper : IFileSystemWrapper
{
    /// <summary>
    /// Writes all text to a file asynchronously
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <param name="contents">The contents to write to the file</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task WriteAllTextAsync(string path, string? contents)
    {
        return System.IO.File.WriteAllTextAsync(path, contents);
    }

    
    /// <summary>
    /// Reads all text from a file asynchronously
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <returns>The contents of the file as a string</returns>
    public Task<string> ReadAllTextAsync(string path)
    {
        return System.IO.File.ReadAllTextAsync(path);
    }

    
    /// <summary>
    /// Checks if a file exists at the specified path
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <returns>>true if the file exists; otherwise, false</returns>
    public bool Exists(string? path)
    {
        return System.IO.File.Exists(path);
    }
}