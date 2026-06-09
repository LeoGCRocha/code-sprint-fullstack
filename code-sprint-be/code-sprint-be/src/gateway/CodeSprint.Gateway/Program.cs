using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddCodeSprintAuth();

// TODO: Validate this....
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", p => p.RequireAuthenticatedUser());

    options.AddPolicy("problems:write", p =>
    {
        p.RequireAuthenticatedUser()
            .RequireClaim("permissions", "write:problems");
    });

    options.FallbackPolicy = options.GetPolicy("authenticated");
});

const string frontendCors = "frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCors, policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("perUser", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.FindFirst("sub")?.Value          
                          ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                          ?? "anonymous",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit          = 100,                      
                TokensPerPeriod     = 20,                       
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),  
                AutoReplenishment   = true,
                QueueLimit          = 0                         
            }));
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(context =>
    {
        context.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Remove("X-User-Sub");
           return default; 
        });
    });

var app = builder.Build();

app.UseCors(frontendCors);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapReverseProxy();
app.Run();
