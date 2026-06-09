namespace CodeSprint.Submissions.Domain;

/// <summary>
/// Programming language a submission is written in. The mapping to the judge's
/// language identifiers (e.g. Judge0 <c>language_id</c>) lives in Infrastructure,
/// not in the domain.
/// </summary>
public enum Language { Python, JavaScript, TypeScript, Cpp, Java, CSharp, Go }
