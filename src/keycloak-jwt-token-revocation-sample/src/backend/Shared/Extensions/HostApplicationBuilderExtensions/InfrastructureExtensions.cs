using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Shared.Extensions.HostApplicationBuilderExtensions;

public static class InfrastructureExtensions
{
    public static void AddBackendInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddJwtAuthenticationWithIntrospection();
        builder.Services.AddAuthorization();
        builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
    }

    public static void UseBackendInfrastructure(this WebApplication app)
    {
        app.UseRouting();
        app.UseCors("frontend");
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
