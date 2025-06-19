namespace TourPlanner.Logic.Interfaces;

public interface IWebViewService
{
    Task InitializeAsync();
    Task<string> ExecuteScriptAsync(string script);
    Task<string> CallFunctionAsync(string functionName, params object[] parameters);
    event EventHandler<string>? MessageReceived;
}