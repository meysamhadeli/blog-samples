using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Shared.Extensions.HostApplicationBuilderExtensions;

public static class JwtAuthenticationExtensions
{
    public static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"]
                    ?? throw new InvalidOperationException("Missing configuration 'Keycloak:Authority'.");
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateAudience = false;
            });
    }
}