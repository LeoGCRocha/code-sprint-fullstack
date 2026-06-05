using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeSprint.Problems.Infrastructure;

/// <summary>
/// Design-time factory used by the EF Core CLI (<c>dotnet ef migrations</c>,
/// <c>dotnet ef database update</c>). At runtime the context is created by DI
/// with the connection string injected by Aspire; this factory only supplies a
/// placeholder connection so the tooling can build the model and emit SQL.
/// </summary>
public sealed class ProblemsDbContextFactory : IDesignTimeDbContextFactory<ProblemsDbContext>
{
    public ProblemsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__problemsdb")
            ?? "Host=localhost;Port=5432;Database=problemsdb;Username=codesprint;Password=codesprint";

        var options = new DbContextOptionsBuilder<ProblemsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ProblemsDbContext(options);
    }
}
