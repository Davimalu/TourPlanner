using System.Net;

namespace TourPlanner.Model.Exceptions;

public class ApiServiceException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Content { get; }
    
    public ApiServiceException(string message, HttpStatusCode statusCode, string? content) : base(message)
    {
        StatusCode = statusCode;
        Content = content;
    }
}