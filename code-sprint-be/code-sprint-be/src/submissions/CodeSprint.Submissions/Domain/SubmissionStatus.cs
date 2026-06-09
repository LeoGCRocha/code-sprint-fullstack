namespace CodeSprint.Submissions.Domain;

/// <summary>
/// Lifecycle of a <see cref="Submission"/>. <c>Failed</c> denotes an infrastructure
/// failure of the judge itself — distinct from a <see cref="Verdict.RuntimeError"/>,
/// which is a legitimate outcome of the user's code.
/// </summary>
public enum SubmissionStatus { Pending, Running, Completed, Failed }
