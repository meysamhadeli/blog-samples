using System.Net;

namespace problem.details.sample;

public class CustomException : Exception
{
    public CustomException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest
    ) : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}