using Microsoft.Web.WebView2.Wpf;

namespace TourPlanner.Logic.Interfaces;

public interface IWebViewService
{
    bool IsReady { get; }
    Task InitializeAsync(WebView2 webView);
    Task<string> ExecuteScriptAsync(string script);
    Task<string> CallFunctionAsync(string functionName, params object[] parameters);
    event EventHandler<string>? MessageReceived;
}