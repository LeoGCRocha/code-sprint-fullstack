using CodeSprint.Users.API;
using CodeSprint.Users.API.Endpoints.Me;
using CodeSprint.Users.Infrastructure;
using CodeSprint.Users.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("usersdb")));

services.AddHttpClient();

// Auth0 JWT authentication + authorization, centralized in ServiceDefaults.
builder.AddCodeSprintAuth();

// RabbitMQ client + IIntegrationEventPublisher transport (consumer reads from here).
builder.AddCodeSprintMessaging();

// READ-side heatmap projection consumer (durable queue, manual-ack; projection is a seam).
builder.Services.AddHostedService<HeatmapProjectionConsumer>();

const string frontendCors = "frontend";
services.AddCors(options =>
{
    options.AddPolicy(frontendCors, policy =>
        policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors(frontendCors);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserSyncMiddleware>();

app.MapUsersMe();
app.MapUsersMeSubmissionActivity();

app.Run();
