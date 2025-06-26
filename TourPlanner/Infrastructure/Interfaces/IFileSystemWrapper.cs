namespace TourPlanner.Infrastructure.Interfaces;

public interface IFileSystemWrapper
{
    Task WriteAllTextAsync(string path, string? contents);
    Task<string> ReadAllTextAsync(string path);
    bool Exists(string? path);
}