using Microsoft.EntityFrameworkCore;
using System.Text.Json; // Add this using

namespace TourPlanner.RestServer.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger; // Use the standard logger

    // We inject the logger here
    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (KeyNotFoundException knfEx)
        {
            _logger.LogWarning(knfEx, "Resource not found for request {Path}", httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsync("Resource not found");
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "A database update error occurred. Inner exception: {InnerMessage}", dbEx.InnerException?.Message);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsync("A database error occurred. Check server logs.");
        }
        catch (Exception ex)
        {
            // THIS IS THE MOST IMPORTANT PART
            // It will log the full error with a stack trace to the server's console.
            _logger.LogError(ex, "An unhandled exception has occurred."); 
            
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            // Optionally, return more error detail in development
            var response = new { message = "An unexpected error occurred.", detail = ex.ToString() };
            var jsonResponse = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}