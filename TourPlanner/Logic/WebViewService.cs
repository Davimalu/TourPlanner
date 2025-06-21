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
    
    private WebView2? _webView;
    public event EventHandler<string>? MessageReceived;
    public bool IsReady { get; private set; } = false;

    private readonly ILoggerWrapper _logger;

    public WebViewService()
    {
        _logger = LoggerFactory.GetLogger<WebViewService>();
    }
    
    
    /// <summary>
    /// <para>Ensure CoreWebView2 (the backend for WebView2) is ready - the WPF Control for WebView2 is created instantly, but the underlying browser process (CoreWebView2) starts up asynchronously (and takes longer)</para>
    /// <para>Set up the event handler to receive messages from JavaScript</para>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task InitializeAsync(WebView2 webView)
    {
        if (_webView != null)
        {
            _logger.Warn("WebView is already set. Overwriting with the new WebView.");
        }
        
        _webView = webView ?? throw new ArgumentNullException(nameof(webView), "WebView cannot be null.");
        _logger.Info("WebView has been set successfully.");

        // Ensure that the WebView2 control is initialized
        await _webView.EnsureCoreWebView2Async();
        
        // WebMessageReceived fires whenever JavaScript code running inside WebView calls the window.chrome.webview.postMessage("message") function
        // -> this allows us to receive messages from the JavaScript code running in the WebView
        _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        
        // Set the IsReady flag to true to indicate that the WebView is ready for use
        IsReady = true;
    }


    /// <summary>
    /// Executes a JavaScript script in the WebView and returns the result as a string
    /// </summary>
    /// <param name="script"></param>
    public async Task<string> ExecuteScriptAsync(string script)
    {
        if (_webView?.CoreWebView2 == null)
        {
            _logger.Error("Script execution failed: WebView is not initialized.");
            return string.Empty;
        }

        try
        {
            _logger.Debug($"Executing JavaScript script: {script}");
            return await _webView.CoreWebView2.ExecuteScriptAsync(script);
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
        if (_webView?.CoreWebView2 == null)
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
            return await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
        catch (Exception ex)
        {
            _logger.Error($"Function call failed: {ex.Message}");
            return string.Empty;
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