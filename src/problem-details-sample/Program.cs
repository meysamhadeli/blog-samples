using Microsoft.AspNetCore.Diagnostics;
using problem.details.sample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseStatusCodePages();

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
                (string Title, string Detail, int StatusCode) details = exceptionType switch
                {
                    CustomException customException =>
                    (
                        exceptionType.GetType().Name,
                        exceptionType.Message,
                        context.Response.StatusCode = (int)customException.StatusCode
                    ),
                    _ =>
                    (
                        exceptionType.GetType().Name,
                        exceptionType.Message,
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
                        Status = details.StatusCode
                    }
                });
            }
        }
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run();