using CodeSprint.Submissions.API.Endpoints.Submissions;
using CodeSprint.Submissions.Infrastructure;
using CodeSprint.Submissions.Infrastructure.Judge;
using CodeSprint.Submissions.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Service discovery (resolves http://users-api), telemetry, health checks.
builder.AddServiceDefaults();

// RabbitMQ client + IIntegrationEventPublisher transport (publishes the outbox).
builder.AddCodeSprintMessaging();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddDbContext<SubmissionsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("submissionsdb"))
           .AddInterceptors(new ConvertDomainEventsToOutboxInterceptor()));

// Auth0 JWT authentication + authorization, centralized in ServiceDefaults.
builder.AddCodeSprintAuth();

// Cross-context caller resolution: typed HttpClient against the Users service.
// Aspire service discovery resolves the "users-api" host from AddServiceDefaults.
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<UsersApiClient>(c => c.BaseAddress = new Uri("http://users-api"));

// In-process judge: singleton queue + background worker (Fase 1 stub, no messaging).
builder.Services.AddSingleton<IJudgeQueue, InMemoryJudgeQueue>();
builder.Services.AddHostedService<StubJudgeWorker>();

// Transactional-outbox drain: polls outbox_messages and publishes to RabbitMQ.
builder.Services.AddHostedService<OutboxPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SubmissionsDbContext>();
    await db.Database.MigrateAsync();
}

// NO CORS here — the gateway owns CORS.
app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();

app.MapSubmissions();

app.Run();
