using HybridCacheSample.Endpoints;

namespace HybridCacheSample;

public static class Extensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapProductEndpoints();

        return app;
    }
}