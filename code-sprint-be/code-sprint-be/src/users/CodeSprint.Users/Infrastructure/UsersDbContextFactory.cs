using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeSprint.Users.Infrastructure;

/// <summary>
/// Design-time factory used by the EF Core CLI (<c>dotnet ef migrations</c>,
/// <c>dotnet ef database update</c>). At runtime the context is created by DI
/// with the connection string injected by Aspire; this factory only supplies a
/// placeholder connection so the tooling can build the model and emit SQL.
/// </summary>
public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__usersdb")
            ?? "Host=localhost;Port=5432;Database=usersdb;Username=codesprint;Password=codesprint";

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new UsersDbContext(options);
    }
}
