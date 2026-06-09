using CodeSprint.Submissions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CodeSprint.Submissions.Infrastructure.Judge;

/// <summary>
/// Background worker that drains <see cref="IJudgeQueue"/> and "judges" each
/// submission. Fase 1 STUB: it does not run any code — it waits briefly and
/// records a synthetic Accepted outcome. Each item is processed in its own DI
/// scope so the scoped <see cref="SubmissionsDbContext"/> is not shared across
/// concurrent units of work.
///
/// STUB: replaced in Fase 3 by a real Judge0 integration. The real worker fetches
/// the problem's test cases from the Problems BC, runs the user's code against
/// them, and maps Judge0 statuses to <see cref="Verdict"/>/<see cref="TestCaseStatus"/>.
/// </summary>
public sealed class StubJudgeWorker(IJudgeQueue queue, IServiceScopeFactory scopeFactory, ILogger<StubJudgeWorker> logger)
    : BackgroundService
{
    // STUB: real points come from the Problems BC (problem.Points) in a later phase.
    private const int StubProblemPoints = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var submissionId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await JudgeAsync(submissionId, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error judging submission {SubmissionId}", submissionId);
            }
        }
    }

    private async Task JudgeAsync(Shared.Ids.SubmissionId submissionId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubmissionsDbContext>();

        var submission = await db.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId, ct);
        if (submission is null)
        {
            logger.LogWarning("Submission {SubmissionId} not found; skipping", submissionId);
            return;
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            logger.LogInformation("Submission {SubmissionId} not Pending ({Status}); skipping",
                submissionId, submission.Status);
            return;
        }

        var startResult = submission.Start();
        if (startResult.IsFailure)
        {
            logger.LogWarning("Could not start submission {SubmissionId}: {Error}",
                submissionId, startResult.Error.Message);
            return;
        }

        await db.SaveChangesAsync(ct);

        try
        {
            // STUB: simulate judge latency. Replaced by Judge0 execution in Fase 3.
            await Task.Delay(1500, ct);

            // STUB: synthetic Accepted outcome with a single hidden, passing test.
            // Real test cases (and their hidden flag) come from the Problems BC.
            var outcome = new JudgeOutcome(
                Verdict.Accepted,
                RuntimeMs: 42,
                MemoryKb: 1024,
                Results:
                [
                    new TestCaseResult(
                        ordinal: 1,
                        status: TestCaseStatus.Passed,
                        runtimeMs: 42,
                        memoryKb: 1024,
                        isHidden: true,
                        actualOutput: null),
                ]);

            // First accepted solve = no PRIOR accepted+completed submission for this
            // user+problem (the current one is still Running, so it is excluded).
            var hasPriorAcceptedSolve = await db.Submissions.AnyAsync(s =>
                s.UserId == submission.UserId &&
                s.ProblemId == submission.ProblemId &&
                s.Status == SubmissionStatus.Completed &&
                s.Evaluation!.Verdict == Verdict.Accepted, ct);

            var completeResult = submission.Complete(outcome, StubProblemPoints, isFirstAcceptedSolve: !hasPriorAcceptedSolve);
            if (completeResult.IsFailure)
            {
                logger.LogWarning("Could not complete submission {SubmissionId}: {Error}",
                    submissionId, completeResult.Error.Message);
                return;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Judged submission {SubmissionId}: {Verdict}", submissionId, outcome.Verdict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Judge failed for submission {SubmissionId}; marking Failed", submissionId);

            // Best-effort: record the infrastructure failure on the aggregate.
            try
            {
                var failResult = submission.Fail(ex.Message);
                if (failResult.IsSuccess)
                    await db.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception saveEx)
            {
                logger.LogError(saveEx, "Could not persist Failed state for submission {SubmissionId}", submissionId);
            }
        }
    }
}
