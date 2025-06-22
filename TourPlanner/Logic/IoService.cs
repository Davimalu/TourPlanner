using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Logic;

public class IoService : IIoService
{
    /// <summary>
    /// Opens a file save dialog with the specified filter, title, and default path.
    /// </summary>
    /// <param name="filter">The filter for the file types to be displayed in the dialog, e.g., "Text files (*.txt)|*.txt|All files (*.*)|*.*"</param>
    /// <param name="title">Title of the dialog, e.g., "Save your file"</param>
    /// <param name="defaultPath">Default path to be displayed in the dialog, e.g., "C:\\"</param>
    /// <returns>Returns the selected file path if the user clicks OK, otherwise returns an empty string.</returns>
    public string OpenFileSaveDialog(string filter, string title, string defaultPath)
    {
        // Create a SaveFileDialog instance
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultPath
        };

        // Show the dialog and check if the user clicked OK
        bool? result = saveFileDialog.ShowDialog();
        if (result == true)
        {
            return saveFileDialog.FileName;
        }

        // Return an empty string if the dialog was canceled
        return string.Empty; 
    }

    
    /// <summary>
    /// Opens a file open dialog with the specified filter, title, and default path.
    /// </summary>
    /// <param name="filter">The filter for the file types to be displayed in the dialog, e.g., "Text files (*.txt)|*.txt|All files (*.*)|*.*"</param>
    /// <param name="title">Title of the dialog, e.g., "Open a file"</param>
    /// <param name="defaultPath">Default path to be displayed in the dialog, e.g., "C:\\"</param>
    /// <returns>Returns the selected file path if the user clicks OK, otherwise returns an empty string.</returns>
    public string OpenFileOpenDialog(string filter, string title, string defaultPath)
    {
        // Create an OpenFileDialog instance
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultPath
        };

        // Show the dialog and check if the user clicked OK
        bool? result = openFileDialog.ShowDialog();
        if (result == true)
        {
            return openFileDialog.FileName;
        }

        // Return an empty string if the dialog was canceled
        return string.Empty;
    }
}