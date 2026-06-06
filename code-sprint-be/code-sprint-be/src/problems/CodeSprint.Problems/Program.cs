using CodeSprint.Problems.API.Endpoints.Problems;
using CodeSprint.Problems.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddDbContext<ProblemsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("problemsdb")));

// Auth0 JWT authentication + authorization, centralized in ServiceDefaults.
builder.AddCodeSprintAuth();

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
    await ProblemSeeder.SeedAsync(db);
}

app.UseCors(frontendCors);
app.UseAuthentication();
app.UseAuthorization();

app.MapProblems();

app.Run();
