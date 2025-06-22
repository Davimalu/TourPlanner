namespace TourPlanner.Logic.Interfaces;

public interface IIoService
{
    string OpenFileSaveDialog(string filter, string title, string defaultPath);
    string OpenFileOpenDialog(string filter, string title, string defaultPath);
}