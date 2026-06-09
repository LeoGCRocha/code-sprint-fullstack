using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeSprint.Submissions.Infrastructure;

/// <summary>
/// Design-time factory used by the EF Core CLI (<c>dotnet ef migrations</c>,
/// <c>dotnet ef database update</c>). At runtime the context is created by DI
/// with the connection string injected by Aspire; this factory only supplies a
/// placeholder connection so the tooling can build the model and emit SQL.
/// </summary>
public sealed class SubmissionsDbContextFactory : IDesignTimeDbContextFactory<SubmissionsDbContext>
{
    public SubmissionsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__submissionsdb")
            ?? "Host=localhost;Port=5432;Database=submissionsdb;Username=codesprint;Password=codesprint";

        var options = new DbContextOptionsBuilder<SubmissionsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SubmissionsDbContext(options);
    }
}
