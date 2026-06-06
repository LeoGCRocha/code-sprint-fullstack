# Problems Domain Scaffold Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Scaffold the Problems bounded context with domain model, EF Core persistence, and three REST endpoints (POST create, GET list published, GET single published by slug).

**Architecture:** Vertical slice mirroring the Users BC — Domain layer (pure C#), Infrastructure layer (EF Core + Postgres), API layer (Minimal API endpoints). `Problem` is the single aggregate owning `Example`, `Tag`, and `TestCase` collections via EF `OwnsMany`. A fourth "publish" endpoint is included because the GET endpoints filter to `IsPublished = true` — without it the list returns empty on a fresh system.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, EF Core 10, Npgsql, xUnit, .NET Aspire

---

## File Map

**Create:**
```
code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/
  Domain/
    Difficulty.cs
    Example.cs
    Tag.cs
    TestCase.cs
    Slug.cs
    Problem.cs
    Events/ProblemCreated.cs
    Events/ProblemPublished.cs
  Infrastructure/
    ProblemsDbContext.cs
    ProblemsDbContextFactory.cs
    ProblemConfiguration.cs
  API/Endpoints/Problems/
    CreateProblemRequest.cs
    ProblemResponse.cs
    ProblemsModule.cs

code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/
  CodeSprint.Problems.Tests.csproj
  Domain/SlugTests.cs
  Domain/ProblemTests.cs
```

**Modify:**
```
code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/
  CodeSprint.Problems.csproj     ← add packages + Shared reference
  Program.cs                     ← wire DbContext, auth, CORS, endpoints, migrations
  appsettings.json               ← add ConnectionStrings placeholder

code-sprint-be/code-sprint-be/src/aspire/CodeSprint.AppHost/
  AppHost.cs                     ← register problemsdb + problems-api
  CodeSprint.AppHost.csproj      ← add Problems project reference
```

---

## Task 1: Add NuGet packages and Shared project reference

**Files:**
- Modify: `code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj`

- [ ] **Step 1: Replace the csproj content**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <LangVersion>latest</LangVersion>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <PackageId>CodeSprint.Problems</PackageId>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\shared\CodeSprint.Shared\CodeSprint.Shared.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.8" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.8" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.8">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.2" />
    </ItemGroup>

</Project>
```

- [ ] **Step 2: Restore packages**

```
dotnet restore code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj
```

Expected: no errors, packages resolved.

- [ ] **Step 3: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj
git commit -m "chore(problems): add EF Core, Npgsql, JWT, Shared packages"
```

---

## Task 2: Domain — Difficulty enum, Example, Tag, TestCase

**Files:**
- Create: `Domain/Difficulty.cs`
- Create: `Domain/Example.cs`
- Create: `Domain/Tag.cs`
- Create: `Domain/TestCase.cs`

- [ ] **Step 1: Create `Domain/Difficulty.cs`**

```csharp
namespace CodeSprint.Problems.Domain;

public enum Difficulty { Easy, Medium, Hard }
```

- [ ] **Step 2: Create `Domain/Example.cs`**

```csharp
namespace CodeSprint.Problems.Domain;

public sealed class Example
{
    public Example() { }

    public Example(int ordinal, string input, string output, string? explanation)
    {
        Ordinal = ordinal;
        Input = input;
        Output = output;
        Explanation = explanation;
    }

    public int Ordinal { get; init; }
    public string Input { get; init; } = string.Empty;
    public string Output { get; init; } = string.Empty;
    public string? Explanation { get; init; }
}
```

- [ ] **Step 3: Create `Domain/Tag.cs`**

```csharp
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed class Tag : ValueObject
{
    public string Value { get; }

    private Tag(string value) => Value = value;

    public static Result<Tag> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Error.Validation("tag.empty", "Tag cannot be empty");

        var value = raw.Trim();
        if (value.Length > 50)
            return Error.Validation("tag.tooLong", "Tag must be 50 characters or fewer");

        return new Tag(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

- [ ] **Step 4: Create `Domain/TestCase.cs`**

```csharp
namespace CodeSprint.Problems.Domain;

public sealed class TestCase
{
    public TestCase() { }

    public TestCase(int ordinal, string input, string expectedOutput, bool isHidden)
    {
        Ordinal = ordinal;
        Input = input;
        ExpectedOutput = expectedOutput;
        IsHidden = isHidden;
    }

    public int Ordinal { get; init; }
    public string Input { get; init; } = string.Empty;
    public string ExpectedOutput { get; init; } = string.Empty;
    public bool IsHidden { get; init; }
}
```

- [ ] **Step 5: Build to verify no errors**

```
dotnet build code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/Domain/
git commit -m "feat(problems): add Difficulty enum, Example, Tag, TestCase domain types"
```

---

## Task 3: Domain — Slug value object + tests

**Files:**
- Create: `Domain/Slug.cs`
- Create: `CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj`
- Create: `CodeSprint.Problems.Tests/Domain/SlugTests.cs`

- [ ] **Step 1: Create the test project**

```
dotnet new xunit -n CodeSprint.Problems.Tests -o "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests" --framework net10.0
```

- [ ] **Step 2: Add project reference to the test csproj**

Edit `code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj` and add:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\CodeSprint.Problems\CodeSprint.Problems.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Write failing `SlugTests.cs`**

Create `code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/Domain/SlugTests.cs`:

```csharp
using CodeSprint.Problems.Domain;

namespace CodeSprint.Problems.Tests.Domain;

public class SlugTests
{
    [Theory]
    [InlineData("two-sum")]
    [InlineData("valid-parentheses")]
    [InlineData("a")]
    [InlineData("abc-123")]
    public void Create_ValidSlug_Succeeds(string raw)
    {
        var result = Slug.Create(raw);
        Assert.True(result.IsSuccess);
        Assert.Equal(raw, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Has Spaces")]
    [InlineData("UPPERCASE")]
    [InlineData("-leading-hyphen")]
    [InlineData("trailing-hyphen-")]
    [InlineData("double--hyphen")]
    public void Create_InvalidSlug_ReturnsFailure(string? raw)
    {
        var result = Slug.Create(raw);
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("Two Sum", "two-sum")]
    [InlineData("Valid Parentheses!", "valid-parentheses")]
    [InlineData("3Sum", "3sum")]
    [InlineData("Longest Substring Without Repeating Characters", "longest-substring-without-repeating-characters")]
    [InlineData("  extra   spaces  ", "extra-spaces")]
    public void Generate_FromTitle_ProducesExpectedSlug(string title, string expected)
    {
        var result = Slug.Generate(title);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    public void Generate_EmptyTitle_ReturnsFailure()
    {
        var result = Slug.Generate("");
        Assert.True(result.IsFailure);
    }
}
```

- [ ] **Step 4: Run tests — expect failure (Slug not yet defined)**

```
dotnet test "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj"
```

Expected: Build error — `Slug` does not exist.

- [ ] **Step 5: Create `Domain/Slug.cs`**

```csharp
using System.Text.RegularExpressions;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed partial class Slug : ValueObject
{
    public string Value { get; }

    private Slug(string value) => Value = value;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();

    public static Result<Slug> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Error.Validation("slug.empty", "Slug is required");

        if (raw.Length > 100)
            return Error.Validation("slug.tooLong", "Slug must be 100 characters or fewer");

        if (!SlugRegex().IsMatch(raw))
            return Error.Validation("slug.invalid",
                "Slug must be lowercase alphanumeric with single hyphens between words");

        return new Slug(raw);
    }

    public static Result<Slug> Generate(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("slug.emptyTitle", "Title is required to generate a slug");

        var normalized = title.Trim().ToLowerInvariant();
        var slugged = NonAlphanumericRegex().Replace(normalized, "-").Trim('-');

        // Collapse consecutive hyphens that survived the replace
        while (slugged.Contains("--"))
            slugged = slugged.Replace("--", "-");

        return Create(slugged);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

- [ ] **Step 6: Run tests — expect pass**

```
dotnet test "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 7: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/
git commit -m "feat(problems): add Slug value object with Create and Generate; add test project"
```

---

## Task 4: Domain — Problem aggregate + domain events + tests

**Files:**
- Create: `Domain/Events/ProblemCreated.cs`
- Create: `Domain/Events/ProblemPublished.cs`
- Create: `Domain/Problem.cs`
- Create: `CodeSprint.Problems.Tests/Domain/ProblemTests.cs`

- [ ] **Step 1: Write failing `ProblemTests.cs`**

Create `code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/Domain/ProblemTests.cs`:

```csharp
using CodeSprint.Problems.Domain;
using CodeSprint.Problems.Domain.Events;

namespace CodeSprint.Problems.Tests.Domain;

public class ProblemTests
{
    private static Slug ValidSlug() => Slug.Create("two-sum").Value;
    private static List<Example> OneExample() =>
        [new Example(1, "input", "output", null)];

    [Fact]
    public void Create_ValidArgs_Succeeds()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 50, estimatedMinutes: 10,
            tags: [Tag.Create("Math").Value],
            description: "A problem",
            notes: ["Note 1"],
            inputFormat: ["Line 1: N"],
            constraints: ["1 <= N <= 100"],
            examples: OneExample());

        Assert.True(result.IsSuccess);
        Assert.Equal("Two Sum", result.Value.Title);
        Assert.Equal(50, result.Value.Points);
        Assert.False(result.Value.IsPublished);
        Assert.Single(result.Value.DomainEvents.OfType<ProblemCreated>());
    }

    [Fact]
    public void Create_ZeroPoints_ReturnsFailure()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 0, estimatedMinutes: 10,
            tags: [],
            description: "desc",
            notes: [],
            inputFormat: [],
            constraints: [],
            examples: OneExample());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_NoExamples_ReturnsFailure()
    {
        var result = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy,
            points: 50, estimatedMinutes: 10,
            tags: [],
            description: "desc",
            notes: [],
            inputFormat: [],
            constraints: [],
            examples: []);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SetExamples_EmptyList_ReturnsFailure()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        var result = problem.SetExamples([]);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void SetExamples_AssignsOrdinalsOneIndexed()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        problem.SetExamples([
            new Example(0, "a", "b", null),
            new Example(0, "c", "d", null),
        ]);

        Assert.Equal(1, problem.Examples[0].Ordinal);
        Assert.Equal(2, problem.Examples[1].Ordinal);
    }

    [Fact]
    public void Publish_WithTestCases_RaisesProblemPublished()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        problem.SetTestCases([new TestCase(1, "in", "out", true)]);
        problem.ClearDomainEvents();

        var result = problem.Publish();

        Assert.True(result.IsSuccess);
        Assert.True(problem.IsPublished);
        Assert.Single(problem.DomainEvents.OfType<ProblemPublished>());
    }

    [Fact]
    public void Publish_WithNoTestCases_ReturnsFailure()
    {
        var problem = Problem.Create(
            ValidSlug(), "Two Sum", Difficulty.Easy, 50, 10,
            [], "desc", [], [], [], OneExample()).Value;

        var result = problem.Publish();

        Assert.True(result.IsFailure);
    }
}
```

- [ ] **Step 2: Run tests — expect build failure (Problem not defined)**

```
dotnet test "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj"
```

Expected: Build error.

- [ ] **Step 3: Create `Domain/Events/ProblemCreated.cs`**

```csharp
using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain.Events;

public sealed record ProblemCreated(ProblemId ProblemId) : IDomainEvent;
```

- [ ] **Step 4: Create `Domain/Events/ProblemPublished.cs`**

```csharp
using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain.Events;

public sealed record ProblemPublished(ProblemId ProblemId) : IDomainEvent;
```

- [ ] **Step 5: Check `IDomainEvent` interface**

Read `code-sprint-be/code-sprint-be/src/shared/CodeSprint.Shared/Primitives/IDomainEvent.cs` to verify interface shape — events must implement it correctly.

The file should contain:
```csharp
namespace CodeSprint.Shared.Primitives;
public interface IDomainEvent { }
```

- [ ] **Step 6: Create `Domain/Problem.cs`**

```csharp
using System.Text.Json;
using CodeSprint.Problems.Domain.Events;
using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain;

public sealed class Problem : AggregateRoot<ProblemId>
{
    private readonly List<Example> _examples = [];
    private readonly List<Tag> _tags = [];
    private readonly List<TestCase> _testCases = [];

    private List<string> _notes = [];
    private List<string> _inputFormat = [];
    private List<string> _constraints = [];

    private Problem(
        ProblemId id,
        Slug slug,
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        string description) : base(id)
    {
        Slug = slug;
        Title = title;
        Difficulty = difficulty;
        Points = points;
        EstimatedMinutes = estimatedMinutes;
        Description = description;
        IsPublished = false;
    }

    public Slug Slug { get; private set; }
    public string Title { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public int Points { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public string Description { get; private set; }
    public bool IsPublished { get; private set; }

    public IReadOnlyList<string> Notes => _notes.AsReadOnly();
    public IReadOnlyList<string> InputFormat => _inputFormat.AsReadOnly();
    public IReadOnlyList<string> Constraints => _constraints.AsReadOnly();
    public IReadOnlyList<Example> Examples => _examples.AsReadOnly();
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
    public IReadOnlyList<TestCase> TestCases => _testCases.AsReadOnly();

    public static Result<Problem> Create(
        Slug slug,
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        IReadOnlyList<Tag> tags,
        string description,
        IReadOnlyList<string> notes,
        IReadOnlyList<string> inputFormat,
        IReadOnlyList<string> constraints,
        IReadOnlyList<Example> examples)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.Validation("problem.title.empty", "Title is required");
        if (points <= 0)
            return Error.Validation("problem.points.invalid", "Points must be greater than 0");
        if (estimatedMinutes <= 0)
            return Error.Validation("problem.estimatedMinutes.invalid", "Estimated minutes must be greater than 0");
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("problem.description.empty", "Description is required");
        if (examples.Count == 0)
            return Error.Validation("problem.examples.empty", "At least one example is required");

        var problem = new Problem(ProblemId.New(), slug, title, difficulty, points, estimatedMinutes, description)
        {
            _notes = [..notes],
            _inputFormat = [..inputFormat],
            _constraints = [..constraints],
        };

        problem._examples.AddRange(examples.Select((e, i) => new Example(i + 1, e.Input, e.Output, e.Explanation)));
        problem._tags.AddRange(tags);
        problem.Raise(new ProblemCreated(problem.Id));

        return problem;
    }

    public Result SetExamples(IReadOnlyList<Example> examples)
    {
        if (examples.Count == 0)
            return Error.Validation("problem.examples.empty", "At least one example is required");

        _examples.Clear();
        _examples.AddRange(examples.Select((e, i) => new Example(i + 1, e.Input, e.Output, e.Explanation)));
        return Result.Success();
    }

    public void SetTestCases(IReadOnlyList<TestCase> testCases)
    {
        _testCases.Clear();
        _testCases.AddRange(testCases.Select((tc, i) => new TestCase(i + 1, tc.Input, tc.ExpectedOutput, tc.IsHidden)));
    }

    public Result Publish()
    {
        if (_testCases.Count == 0)
            return Error.Validation("problem.publish.noTestCases", "Cannot publish a problem with no test cases");

        IsPublished = true;
        Raise(new ProblemPublished(Id));
        return Result.Success();
    }

    public void Edit(
        string title,
        Difficulty difficulty,
        int points,
        int estimatedMinutes,
        IReadOnlyList<Tag> tags,
        string description,
        IReadOnlyList<string> notes,
        IReadOnlyList<string> inputFormat,
        IReadOnlyList<string> constraints)
    {
        Title = title;
        Difficulty = difficulty;
        Points = points;
        EstimatedMinutes = estimatedMinutes;
        Description = description;
        _notes = [..notes];
        _inputFormat = [..inputFormat];
        _constraints = [..constraints];
        _tags.Clear();
        _tags.AddRange(tags);
    }
}
```

- [ ] **Step 7: Run tests — expect pass**

```
dotnet test "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 8: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/
git commit -m "feat(problems): add Problem aggregate root, ProblemCreated, ProblemPublished events"
```

---

## Task 5: Infrastructure — DbContext, Factory, EF configuration

**Files:**
- Create: `Infrastructure/ProblemsDbContext.cs`
- Create: `Infrastructure/ProblemsDbContextFactory.cs`
- Create: `Infrastructure/ProblemConfiguration.cs`

- [ ] **Step 1: Create `Infrastructure/ProblemsDbContext.cs`**

```csharp
using CodeSprint.Problems.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Problems.Infrastructure;

public sealed class ProblemsDbContext(DbContextOptions<ProblemsDbContext> options) : DbContext(options)
{
    public DbSet<Problem> Problems => Set<Problem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("problems");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProblemsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 2: Create `Infrastructure/ProblemsDbContextFactory.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeSprint.Problems.Infrastructure;

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
```

- [ ] **Step 3: Create `Infrastructure/ProblemConfiguration.cs`**

```csharp
using System.Text.Json;
using CodeSprint.Problems.Domain;
using CodeSprint.Shared.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeSprint.Problems.Infrastructure;

public sealed class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.ToTable("problems");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("problem_id")
            .HasConversion(id => id.Value, value => ProblemId.From(value))
            .ValueGeneratedNever();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasConversion(s => s.Value, value => Slug.Create(value).Value)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.Points)
            .HasColumnName("points")
            .IsRequired();

        builder.Property(p => p.EstimatedMinutes)
            .HasColumnName("estimated_minutes")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(p => p.IsPublished)
            .HasColumnName("is_published")
            .IsRequired();

        builder.Property<List<string>>("_notes")
            .HasColumnName("notes")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<List<string>>("_inputFormat")
            .HasColumnName("input_format")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<List<string>>("_constraints")
            .HasColumnName("constraints")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts)!)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Examples, examples =>
        {
            examples.ToTable("problem_examples");
            examples.WithOwner().HasForeignKey("problem_id");
            examples.HasKey("problem_id", nameof(Example.Ordinal));
            examples.Property(e => e.Ordinal).HasColumnName("ordinal");
            examples.Property(e => e.Input).HasColumnName("input").IsRequired();
            examples.Property(e => e.Output).HasColumnName("output").IsRequired();
            examples.Property(e => e.Explanation).HasColumnName("explanation");
        });
        builder.Navigation(p => p.Examples)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Tags, tags =>
        {
            tags.ToTable("problem_tags");
            tags.WithOwner().HasForeignKey("problem_id");
            tags.HasKey("problem_id", nameof(Tag.Value));
            tags.Property(t => t.Value)
                .HasColumnName("tag")
                .HasMaxLength(50)
                .IsRequired();
        });
        builder.Navigation(p => p.Tags)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.TestCases, testCases =>
        {
            testCases.ToTable("problem_test_cases");
            testCases.WithOwner().HasForeignKey("problem_id");
            testCases.HasKey("problem_id", nameof(TestCase.Ordinal));
            testCases.Property(tc => tc.Ordinal).HasColumnName("ordinal");
            testCases.Property(tc => tc.Input).HasColumnName("input").IsRequired();
            testCases.Property(tc => tc.ExpectedOutput).HasColumnName("expected_output").IsRequired();
            testCases.Property(tc => tc.IsHidden).HasColumnName("is_hidden").IsRequired();
        });
        builder.Navigation(p => p.TestCases)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

- [ ] **Step 4: Build to verify**

```
dotnet build code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 5: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/Infrastructure/
git commit -m "feat(problems): add ProblemsDbContext, factory, and EF Core ProblemConfiguration"
```

---

## Task 6: EF migration

**Files:**
- Generated: `Infrastructure/Migrations/` (auto-generated)

> Run commands from the `CodeSprint.Problems` project directory.

- [ ] **Step 1: Add the initial migration**

```
dotnet ef migrations add InitialProblems --project "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj"
```

Expected: `Migrations/` folder created with `<timestamp>_InitialProblems.cs` and snapshot.

- [ ] **Step 2: Inspect the generated migration**

Open the generated `Up()` method. Verify it creates:
- `problems.problems` table with all scalar columns
- `problems.problem_examples` table with `(problem_id, ordinal)` composite PK
- `problems.problem_tags` table with `(problem_id, tag)` composite PK
- `problems.problem_test_cases` table with `(problem_id, ordinal)` composite PK

If anything looks wrong (missing columns, wrong types), fix `ProblemConfiguration.cs` and re-run with `--force`.

- [ ] **Step 3: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/Infrastructure/Migrations/
git commit -m "feat(problems): add InitialProblems EF migration"
```

---

## Task 7: Aspire — register problemsdb and problems-api

**Files:**
- Modify: `code-sprint-be/code-sprint-be/src/aspire/CodeSprint.AppHost/AppHost.cs`
- Modify: `code-sprint-be/code-sprint-be/src/aspire/CodeSprint.AppHost/CodeSprint.AppHost.csproj`

- [ ] **Step 1: Add Problems project reference to AppHost.csproj**

Add inside the `<ItemGroup>` that already has the Users reference:

```xml
<ProjectReference Include="..\..\problems\CodeSprint.Problems\CodeSprint.Problems.csproj" />
```

- [ ] **Step 2: Update `AppHost.cs`**

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("codesprint-pgdata")
    .WithPgAdmin();

var usersDb = postgres.AddDatabase("usersdb");
var problemsDb = postgres.AddDatabase("problemsdb");

builder.AddProject<Projects.CodeSprint_Users>("users-api")
       .WithReference(usersDb)
       .WaitFor(usersDb);

builder.AddProject<Projects.CodeSprint_Problems>("problems-api")
       .WithReference(problemsDb)
       .WaitFor(problemsDb);

builder.Build().Run();
```

- [ ] **Step 3: Build AppHost to verify**

```
dotnet build "code-sprint-be/code-sprint-be/src/aspire/CodeSprint.AppHost/CodeSprint.AppHost.csproj"
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```
git add code-sprint-be/code-sprint-be/src/aspire/
git commit -m "feat(aspire): register problemsdb and problems-api"
```

---

## Task 8: API endpoints + Program.cs wiring

**Files:**
- Create: `API/Endpoints/Problems/CreateProblemRequest.cs`
- Create: `API/Endpoints/Problems/ProblemResponse.cs`
- Create: `API/Endpoints/Problems/ProblemsModule.cs`
- Modify: `Program.cs`
- Modify: `appsettings.json`

- [ ] **Step 1: Create `API/Endpoints/Problems/CreateProblemRequest.cs`**

```csharp
namespace CodeSprint.Problems.API.Endpoints.Problems;

public record CreateExampleRequest(string Input, string Output, string? Explanation);

public record CreateProblemRequest(
    string? Slug,
    string Title,
    string Difficulty,
    int Points,
    int EstimatedMinutes,
    List<string> Tags,
    string Description,
    List<string> Notes,
    List<string> InputFormat,
    List<string> Constraints,
    List<CreateExampleRequest> Examples);
```

- [ ] **Step 2: Create `API/Endpoints/Problems/ProblemResponse.cs`**

```csharp
namespace CodeSprint.Problems.API.Endpoints.Problems;

public record ExampleResponse(string Input, string Output, string? Explanation);

public record ProblemResponse(
    string Id,
    string Slug,
    string Title,
    string Difficulty,
    int Points,
    string EstimatedTime,
    List<string> Tags,
    string Description,
    List<string> Notes,
    List<string> InputFormat,
    List<string> Constraints,
    List<ExampleResponse> Examples,
    int SolvedCount);
```

- [ ] **Step 3: Create `API/Endpoints/Problems/ProblemsModule.cs`**

```csharp
using CodeSprint.Problems.Domain;
using CodeSprint.Problems.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Problems.API.Endpoints.Problems;

public static class ProblemsModule
{
    public static IEndpointRouteBuilder MapProblems(this IEndpointRouteBuilder app)
    {
        app.MapPost("/problems", Create).RequireAuthorization();
        app.MapGet("/problems", List);
        app.MapGet("/problems/{slug}", GetBySlug);
        app.MapPost("/problems/{slug}/publish", Publish).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Create(CreateProblemRequest req, ProblemsDbContext db)
    {
        // Resolve slug: use provided or generate from title
        var slugResult = string.IsNullOrWhiteSpace(req.Slug)
            ? Slug.Generate(req.Title)
            : Slug.Create(req.Slug);

        if (slugResult.IsFailure)
            return Results.BadRequest(slugResult.Error.Message);

        var slug = slugResult.Value;

        // Uniqueness check
        var taken = await db.Problems.AnyAsync(p => p.Slug == slug);
        if (taken)
            return Results.Conflict($"Slug '{slug.Value}' is already taken");

        // Parse difficulty
        if (!Enum.TryParse<Difficulty>(req.Difficulty, ignoreCase: true, out var difficulty))
            return Results.BadRequest($"Invalid difficulty '{req.Difficulty}'. Valid: Easy, Medium, Hard");

        // Parse tags
        var tags = new List<Tag>();
        foreach (var raw in req.Tags)
        {
            var tagResult = Tag.Create(raw);
            if (tagResult.IsFailure)
                return Results.BadRequest(tagResult.Error.Message);
            tags.Add(tagResult.Value);
        }

        // Build examples
        var examples = req.Examples
            .Select(e => new Example(0, e.Input, e.Output, e.Explanation))
            .ToList();

        var problemResult = Problem.Create(
            slug, req.Title, difficulty, req.Points, req.EstimatedMinutes,
            tags, req.Description, req.Notes, req.InputFormat, req.Constraints, examples);

        if (problemResult.IsFailure)
            return Results.BadRequest(problemResult.Error.Message);

        db.Problems.Add(problemResult.Value);
        await db.SaveChangesAsync();

        return Results.Created($"/problems/{slug.Value}", ToResponse(problemResult.Value));
    }

    private static async Task<IResult> List(ProblemsDbContext db)
    {
        var problems = await db.Problems
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderBy(p => p.Title)
            .ToListAsync();

        return Results.Ok(problems.Select(ToResponse));
    }

    private static async Task<IResult> GetBySlug(string slug, ProblemsDbContext db)
    {
        var slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
            return Results.NotFound();

        var problem = await db.Problems
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slugResult.Value && p.IsPublished);

        return problem is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(problem));
    }

    private static async Task<IResult> Publish(string slug, ProblemsDbContext db)
    {
        var slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
            return Results.NotFound();

        var problem = await db.Problems
            .FirstOrDefaultAsync(p => p.Slug == slugResult.Value);

        if (problem is null)
            return Results.NotFound();

        var result = problem.Publish();
        if (result.IsFailure)
            return Results.BadRequest(result.Error.Message);

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static ProblemResponse ToResponse(Problem p) => new(
        Id: p.Id.ToString(),
        Slug: p.Slug.Value,
        Title: p.Title,
        Difficulty: p.Difficulty.ToString().ToLowerInvariant(),
        Points: p.Points,
        EstimatedTime: $"{p.EstimatedMinutes} min",
        Tags: p.Tags.Select(t => t.Value).ToList(),
        Description: p.Description,
        Notes: p.Notes.ToList(),
        InputFormat: p.InputFormat.ToList(),
        Constraints: p.Constraints.ToList(),
        Examples: p.Examples
            .OrderBy(e => e.Ordinal)
            .Select(e => new ExampleResponse(e.Input, e.Output, e.Explanation))
            .ToList(),
        SolvedCount: 0);
}
```

- [ ] **Step 4: Update `appsettings.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth0": {
    "Authority": "",
    "Audience": ""
  },
  "Cors": {
    "FrontendOrigin": "http://localhost:3000"
  }
}
```

- [ ] **Step 5: Update `Program.cs`**

```csharp
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
```

- [ ] **Step 6: Full build**

```
dotnet build "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems/CodeSprint.Problems.csproj"
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 7: Run all tests**

```
dotnet test "code-sprint-be/code-sprint-be/src/problems/CodeSprint.Problems.Tests/CodeSprint.Problems.Tests.csproj"
```

Expected: All tests pass.

- [ ] **Step 8: Commit**

```
git add code-sprint-be/code-sprint-be/src/problems/
git commit -m "feat(problems): scaffold 4 REST endpoints (create, list, get, publish) with Program.cs wiring"
```

---

## Known gaps (not in scope for this scaffold)

- **`SolvedCount` always returns 0** — `PROBLEM_SOLVE_STAT` + `PROBLEM_FIRST_SOLVE` read-model tables not yet created. Needs `SubmissionEvaluated` handler.
- **No `ProblemStatus` (per-user state)** — `start`/`continue`/`review` belongs to Submission BC. BFF composes it onto the response at call time.
- **No admin role check on `POST /problems` and `POST /problems/{slug}/publish`** — any authenticated user can create/publish. Tighten when an admin role system exists.
- **`TestCases.Any()` invariant on Publish** — blocks publish on a freshly created problem. Add test cases via a separate endpoint before publishing.
