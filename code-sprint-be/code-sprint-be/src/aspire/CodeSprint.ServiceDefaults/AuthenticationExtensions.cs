using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

// Centralized Auth0 JWT bearer authentication for every CodeSprint service.
// Validation still runs in each service (zero-trust), but the setup lives in one place.
// Configuration is read from the "Auth0" section: Auth0:Authority and Auth0:Audience.
public static class AuthenticationExtensions
{
    public static TBuilder AddCodeSprintAuth<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority        = builder.Configuration["Auth0:Authority"];
            options.Audience         = builder.Configuration["Auth0:Audience"];
            options.MapInboundClaims = false;
        });

        builder.Services.AddAuthorization();

        return builder;
    }
}
