using Microsoft.AspNetCore.Diagnostics;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";

        if (context.RequestServices.GetService<IProblemDetailsService>() is { } problemDetailsService)
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exceptionType = exceptionHandlerFeature?.Error;

            if (exceptionType is not null)
            {
                (string Detail, string Type, string Title, int StatusCode) details = exceptionType switch
                {
                    CustomException customException =>
                    (
                        exceptionType.Message,
                        exceptionType.GetType().ToString(),
                        exceptionType.GetType().Name,
                        context.Response.StatusCode = (int)customException.StatusCode
                    ),
                    _ =>
                    (
                        exceptionType.Message,
                        exceptionType.GetType().ToString(),
                        exceptionType.GetType().Name,
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError
                    )
                };

                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails =
                    {
                        Title = details.Title,
                        Detail = details.Detail,
                        Type = details.Type,
                        Status = details.StatusCode
                    }
                });
            }
        }
    });
});

app.MapControllers();

app.Run();