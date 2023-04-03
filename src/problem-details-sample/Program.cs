using Microsoft.AspNetCore.Diagnostics;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
                (string Title, string Detail, string Type, int StatusCode) details = exceptionType switch
                {
                    CustomException customException =>
                    (
                        exceptionType.GetType().Name,
                        exceptionType.Message,
                        "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
                        context.Response.StatusCode = (int)customException.StatusCode
                    ),
                    _ =>
                    (
                        exceptionType.GetType().Name,
                        exceptionType.Message,
                        "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1",
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