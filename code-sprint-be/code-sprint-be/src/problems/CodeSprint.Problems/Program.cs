using CodeSprint.Problems.API.Endpoints.Problems;
using CodeSprint.Problems.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProblemsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("problemsdb")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Authority"];
    options.Audience  = builder.Configuration["Auth0:Audience"];
    options.MapInboundClaims = false;
});
builder.Services.AddAuthorization();

const string frontendCors = "frontend";
builder.Services.AddCors(options =>
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
    var db = scope.ServiceProvider.GetRequiredService<ProblemsDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors(frontendCors);
app.UseAuthentication();
app.UseAuthorization();

app.MapProblems();

app.Run();
