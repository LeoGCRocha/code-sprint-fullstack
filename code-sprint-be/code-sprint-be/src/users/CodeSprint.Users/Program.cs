using CodeSprint.Users.API;
using CodeSprint.Users.API.Endpoints.Me;
using CodeSprint.Users.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("usersdb")));

services.AddHttpClient();

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Authority"];
    options.Audience  = builder.Configuration["Auth0:Audience"];
    options.MapInboundClaims = false;
});
services.AddAuthorization();

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

app.Run();
