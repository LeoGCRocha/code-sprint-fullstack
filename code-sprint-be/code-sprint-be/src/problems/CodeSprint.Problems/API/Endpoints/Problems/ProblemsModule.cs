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
        var slugResult = string.IsNullOrWhiteSpace(req.Slug)
            ? Slug.Generate(req.Title)
            : Slug.Create(req.Slug);

        if (slugResult.IsFailure)
            return Results.BadRequest(slugResult.Error.Message);

        var slug = slugResult.Value;

        var taken = await db.Problems.AnyAsync(p => p.Slug == slug);
        if (taken)
            return Results.Conflict($"Slug '{slug.Value}' is already taken");

        if (!Enum.TryParse<Difficulty>(req.Difficulty, ignoreCase: true, out var difficulty))
            return Results.BadRequest($"Invalid difficulty '{req.Difficulty}'. Valid: Easy, Medium, Hard");

        var tags = new List<Tag>();
        foreach (var raw in req.Tags)
        {
            var tagResult = Tag.Create(raw);
            if (tagResult.IsFailure)
                return Results.BadRequest(tagResult.Error.Message);
            tags.Add(tagResult.Value);
        }

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
