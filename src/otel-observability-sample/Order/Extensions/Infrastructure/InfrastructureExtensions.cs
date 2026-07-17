using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Data;
using Order.Orders.Features.CreatingOrder;
using Order.Orders.Features.GettingOrderById;
using Order.Orders.Features.ListingOrders;

namespace Order.Extensions.Infrastructure;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        // Swagger / OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Entity Framework — PostgreSQL
        builder.Services.AddDbContext<OrderDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb"))
        );

        // MediatR
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OrderRoot).Assembly)
        );

        // AutoMapper
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(OrderRoot).Assembly));

        // ---- OpenTelemetry ----
        var serviceVersion = Assembly.GetEntryAssembly()?
            .GetName().Version?.ToString() ?? "1.0.0";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName
                )
            )
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddOtlpExporter()
            )
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter()
            );

        // ---- Service Discovery ----
        builder.Services.AddServiceDiscovery();

        // ---- Health Checks ----
        builder.Services.AddHealthChecks()
            .AddUrlGroup(
                new Uri("http://localhost:5002/healthz"),
                name: "Self Health Check",
                tags: ["healthz"]
            );

        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Map endpoints
        app.MapCreateOrderEndpoint();
        app.MapGetOrderByIdEndpoint();
        app.MapGetOrdersEndpoint();

        // Health check endpoint
        app.MapHealthChecks("/healthz");

        app.UseHttpsRedirection();
        return app;
    }
}
