using System.IO;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using TourPlanner.Infrastructure;
using TourPlanner.Infrastructure.Interfaces;
using TourPlanner.Logic.Interfaces;

namespace TourPlanner.Logic;

public class WebViewService : IWebViewService
{
    /* Generally, a service shouldn't depend on a specific UI component like WebView2. However, this service is SO SPECIFIC to WebView2 that it can't really be used in any other way anyway, so we think it's okay to have it here
    We could add an interface for WebView2, but that would just add unnecessary complexity for this specific use case. */
    
    
    /* Once the main window spawns, it initializes a map using a WebView2 User Control - this user control calls the WebViewService.InitializeAsync method to set the WebView2 control
     * Once the Edit Tour Windows spawns, it also initializes a WebView2 User Control for its map that also calls the WebViewService.InitializeAsync method to set the WebView2 control
     * The problem is that this second call overwrites the first one (the main window's WebView2), which means we loose communication with the main window's WebView2
     * To solve this problem, we keep a reference to the main window's WebView2 in _mainWindowWebView - calling RevertToMainWindowWebViewAsync() will set the _activeWebView to the main window's WebView2 again
     * I'm not entirely happy with this solution, especially since it requires manually calling RevertToMainWindowWebViewAsync() manually, but it seems to be the best way to address this problem without introducing a lot of complexity
     */
    private WebView2? _activeWebView;
    private WebView2? _mainWindowWebView;
    
    public event EventHandler<string>? MessageReceived;
    public bool IsReady { get; private set; } = false;

    private readonly ILoggerWrapper _logger;

    public WebViewService()
    {
        _logger = LoggerFactory.GetLogger<WebViewService>();
    }
    
    
    /// <summary>
    /// Ensure CoreWebView2 (the backend for WebView2) is ready - the WPF Control for WebView2 is created instantly, but the underlying browser process (CoreWebView2) starts up asynchronously (and takes longer>
    /// <list type="bullet">
    /// <item><description>Sets the active WebView2 control (the one controlled by this service) to the provided WebView2 instance</description></item>
    /// <item><description>Sets up an event handler to receive messages from JavaScript code running inside the WebView</description></item>
    /// </list>
    /// <para>The first call to this method should be made from the main window's WebView2 control, which will be used as the main WebView2 control</para>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task InitializeAsync(WebView2 webView)
    {
        if (_activeWebView == null)
        {
            _logger.Debug("First time setting WebView. Setting the provided WebView as the main WebView.");
            
            _mainWindowWebView = webView;
            _activeWebView = webView;
        }
        else
        {
            _logger.Debug("Subsequent time setting WebView. Setting the active WebView to the provided WebView.");
            _activeWebView = webView;
        }
        
        _activeWebView = webView ?? throw new ArgumentNullException(nameof(webView), "WebView cannot be null.");
        _logger.Info("WebView has been set successfully.");

        // Ensure that the WebView2 control is initialized
        await _activeWebView.EnsureCoreWebView2Async();
        
        // WebMessageReceived fires whenever JavaScript code running inside WebView calls the window.chrome.webview.postMessage("message") function
        // -> this allows us to receive messages from the JavaScript code running in the WebView
        _activeWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        
        // Set the IsReady flag to true to indicate that the WebView is ready for use
        IsReady = true;
    }


    /// <summary>
    /// Reverts the active WebView to the main window's WebView
    /// </summary>
    /// <returns>true if the operation was successful, false otherwise</returns>
    public Task<bool> RevertToMainWindowWebViewAsync()
    {
        if (_mainWindowWebView == null)
        {
            _logger.Error("Error reverting to MainWindow's web view: No main window WebView is set.");
            return Task.FromResult(false);
        }
        
        // Remove the event handler from the current active WebView
        if (_activeWebView != null)
        {
            _activeWebView.CoreWebView2.WebMessageReceived -= OnWebMessageReceived;
            _logger.Debug("Removed WebMessageReceived event handler from the current active WebView.");
        }
        
        _activeWebView = _mainWindowWebView;
        _logger.Info("Reverted to main window's WebView successfully.");
        return Task.FromResult(true);
    }


    /// <summary>
    /// Executes a JavaScript script in the WebView and returns the result as a string
    /// </summary>
    /// <param name="script"></param>
    public async Task<string> ExecuteScriptAsync(string script)
    {
        if (_activeWebView?.CoreWebView2 == null)
        {
            _logger.Error("Script execution failed: WebView is not initialized.");
            return string.Empty;
        }

        try
        {
            _logger.Debug($"Executing JavaScript script: {script}");
            return await _activeWebView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            _logger.Error($"Script execution failed: {ex.Message}");
            return string.Empty;
        }
    }


    /// <summary>
    /// Calls a JavaScript function in the WebView and returns the result as a string
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public async Task<string> CallFunctionAsync(string functionName, params object[] parameters)
    {
        if (_activeWebView?.CoreWebView2 == null)
        {
            _logger.Error("Function call failed: WebView is not initialized.");
            return string.Empty;
        }
        
        // C# objects can't be passed directly to JavaScript, so we need to serialize them to JSON
        var jsonParams = parameters.Select(p => JsonSerializer.Serialize(p)).ToArray();
        var script = $"{functionName}({string.Join(",", jsonParams)})";

        try
        {
            _logger.Debug($"Calling JavaScript function: {functionName}");
            return await _activeWebView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            _logger.Error($"Function call failed: {ex.Message}");
            return string.Empty;
        }
    }
    
    
    /// <summary>
    /// Takes a screenshot of the WebView and saves it to the specified file path
    /// </summary>
    /// <param name="filePath">The file path where the screenshot will be saved, e.g., "C:\\screenshot.png"</param>
    /// <returns>>true if the screenshot was taken successfully, false otherwise</returns>
    public async Task<bool> TakeScreenshotAsync(string filePath)
    {
        if (_activeWebView?.CoreWebView2 == null)
        {
            _logger.Error("Screenshot capture failed: WebView is not initialized.");
            return false;
        }

        // Ensure that the WebView2 control is initialized
        await _activeWebView.EnsureCoreWebView2Async(null);
        
        try
        {
            _logger.Debug("Capturing screenshot of the WebView...");
            
            // Make sure the output directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
            
            // Capture screenshot
            await using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await _activeWebView.CoreWebView2.CapturePreviewAsync(
                    CoreWebView2CapturePreviewImageFormat.Png,
                    stream);
            }
            
            _logger.Info($"Screenshot captured successfully and saved to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Screenshot capture failed: {ex.Message}");
            return false;
        }
    }

    
    /// <summary>
    /// Handles messages received from the WebView's JavaScript code
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        // try to get the message as a string
        var message = e.TryGetWebMessageAsString();
        
        if (message != null)
        {
            MessageReceived?.Invoke(this, message);
        }
    }
}