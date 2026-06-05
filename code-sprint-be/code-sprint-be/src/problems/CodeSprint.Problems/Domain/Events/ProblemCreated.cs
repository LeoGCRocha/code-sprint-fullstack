using CodeSprint.Shared.Ids;
using CodeSprint.Shared.Primitives;

namespace CodeSprint.Problems.Domain.Events;

public sealed record ProblemCreated(ProblemId ProblemId) : DomainEvent;
