namespace CodeSprint.Submissions.Domain;

/// <summary>Per-test outcome within a submission's <see cref="Evaluation"/>.</summary>
public enum TestCaseStatus { Passed, Failed, Errored, TimedOut }
