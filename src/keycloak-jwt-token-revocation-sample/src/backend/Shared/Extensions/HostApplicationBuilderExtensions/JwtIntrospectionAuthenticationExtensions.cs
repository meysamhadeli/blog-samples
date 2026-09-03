using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Shared.Extensions.HostApplicationBuilderExtensions;

public static class JwtIntrospectionAuthenticationExtensions
{
    public static void AddJwtAuthenticationWithIntrospection(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("keycloak-introspection", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Keycloak:Authority"]
                    ?? throw new InvalidOperationException("Missing configuration 'Keycloak:Authority'.");
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateTokenWithKeycloakAsync
                };
            });
    }

    private static async Task ValidateTokenWithKeycloakAsync(TokenValidatedContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var clientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
        var authorization = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!AuthenticationHeaderValue.TryParse(authorization, out var authorizationHeader) ||
            !string.Equals(authorizationHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(authorizationHeader.Parameter))
        {
            context.Fail("Missing bearer token");
            return;
        }

        var introspectionUrl = $"{configuration["Keycloak:Authority"]}/protocol/openid-connect/token/introspect";
        using var request = new HttpRequestMessage(HttpMethod.Post, introspectionUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = authorizationHeader.Parameter
            })
        };
        var credentials = $"{configuration["Keycloak:IntrospectionClientId"]}:{configuration["Keycloak:IntrospectionClientSecret"]}";
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

        using var response = await clientFactory
            .CreateClient("keycloak-introspection")
            .SendAsync(request, context.HttpContext.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            context.Fail("Keycloak introspection failed");
            return;
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!document.RootElement.TryGetProperty("active", out var active) || !active.GetBoolean())
            context.Fail("Token is inactive or revoked");
    }
}