# Domain Model — Users & Problems

> Scope: only the **Users** and **Problems** bounded contexts. Submission, Competition,
> and Runner contexts appear here **only** as integration partners — they are not modeled.
> Status: living document. Decisions captured inline as resolved.

---

## 1. The core insight

The frontend `Problem` type (`code-sprint-fe/features/problems/types.ts`) fuses data from
**three** different contexts into one shape. The same happens to "User" across the profile UI.
A field belongs to an aggregate **only if that aggregate is the authority for it and can
enforce invariants on it.** Everything else is a *projection* (a read model) or a *foreign
reference* (an ID).

Applying that test:

| FE `Problem` field | Real owner | Kind |
|---|---|---|
| `title, slug, difficulty, points, tags, description, notes, inputFormat, constraints, examples, estimatedTime` | **Problems BC** | Authored definition (aggregate) |
| `solvedCount` | Problems BC | **Read-model stat** (event-driven, not authored) |
| `status` (`start`/`continue`/`review`) | **Submission BC** (per user) | **Per-user projection**, not a property of the Problem |
| `id` | Problems BC | Identity |

> The FE type is fine **as a read/query model**. The mistake would be persisting it that
> shape on the write side.

---

## 2. Bounded contexts & context map

```
                 SubmissionEvaluated (RabbitMQ, async)
        ┌──────────────────────────────────────────────┐
        │                                               ▼
  ┌───────────────┐   gRPC (read TestCases)    ┌──────────────────┐
  │  Submission   │ ─────────────────────────▶ │   Problems BC    │
  │   (upstream   │                            │  Problem (AR)    │
  │   publisher)  │                            │                  │
  └───────────────┘                            └──────────────────┘
        │  SubmissionEvaluated                          ▲
        ▼                                               │ solvedCount
  ┌──────────────────┐                                  │ read model
  │    Users BC      │ ◀────────────────────────────────┘
  │  User (AR)       │   (Users & Problems NEVER call each other)
  │  PlayerProgress  │
  │  (read model)    │
  └──────────────────┘
```

**Relationships (context mapping):**

- **Submission → Users** and **Submission → Problems**: *Customer–Supplier* via the
  `SubmissionEvaluated` integration event. Submission is upstream; both read models conform
  to its published contract (apply an **Anti-Corruption Layer** when translating the event
  into local model terms — do not leak Submission's vocabulary into User/Problem).
- **Submission → Problems** (gRPC test cases fetch): *Customer–Supplier*; Problems is the
  supplier of the judging data.
- **Users ↔ Problems**: **no relationship.** They share no schema, no object model, no calls.
  Their only connection is indirect, through Submission events.

---

## 3. The layers (per bounded context)

Each service is a vertical slice with the standard four layers. Dependencies point **inward**
(Domain depends on nothing).

| Layer | Responsibility | Examples (Users BC) | Examples (Problems BC) |
|---|---|---|---|
| **Domain** | Aggregates, entities, value objects, domain events, invariants. Pure, no I/O. | `User`, `Handle`, `Email`, `UserRegistered` | `Problem`, `Slug`, `Tag`, `Difficulty`, `Example`, `TestCase` |
| **Application** | Use cases / command + query handlers, orchestration, transaction boundary, ports (interfaces). | `RegisterUser`, `ChangeHandle`, `GetProfile` | `CreateProblem`, `EditProblem`, `PublishProblem`, `SetTestCases`, `GetProblem`, `ListProblems` |
| **Infrastructure** | Adapters: EF Core repos, Postgres, RabbitMQ publish/subscribe, gRPC servers/clients, OAuth. | `UserRepository`, `ProgressionProjectionHandler` | `ProblemRepository`, `ProblemGrpcService`, `SolvedCountProjectionHandler` |
| **API / Presentation** | Transport edge: gRPC service impls, REST controllers if any, DTO mapping. Behind the API Gateway. | gRPC `UsersService` | gRPC `ProblemsService` |

> The **read models** (`PlayerProgression`, `solvedCount`, per-user `ProblemStatus`) are an
> Application/Infrastructure concern (CQRS read side), **not** domain aggregates. They have no
> invariants — they're shaped for queries.

---

## 4. Users bounded context

### 4.1 `User` — aggregate root

Owns only identity/profile — things it validates and enforces uniqueness on.

```
User (AggregateRoot)
├─ UserId            : value object (typed ID, ULID/UUID)   ← identity
├─ Email             : value object (validated, unique)
├─ Handle            : value object ("@johndoe", unique, immutable-ish)
├─ DisplayName       : value object ("John Doe")
├─ Avatar            : value object (initials or URL)
├─ Identities        : list<AuthIdentity>  (OAuth providers: google|github, + local creds)
└─ MemberSince       : timestamp
```

**Invariants:**
- `Email` and `Handle` are globally unique (enforced via repository/uniqueness check in the
  Application layer + DB constraint).
- At least one `AuthIdentity` must exist (you can't have a credential-less account).
- `Handle` matches an allowed pattern; changing it is a deliberate, rule-guarded operation.

**Domain events emitted:** `UserRegistered`, `HandleChanged`, `ProfileUpdated`.

**NOT in the User aggregate:** rank, streak, points, tier (`Expert`), `Top 1%`, achievements,
heatmap, category breakdown. All derived.

### 4.2 `PlayerProgression` — read model (NOT an aggregate)

Projection owned by Users BC, rebuilt/updated from `SubmissionEvaluated` events.

```
PlayerProgression (read model, keyed by UserId)
├─ TotalPoints        ← sum of points from accepted submissions
├─ Rank               ← derived ranking (recompute / windowed)
├─ CurrentStreak      ← consecutive-day activity rule
├─ Tier               ← "Expert" etc., a function of points/rank
├─ Percentile         ← "Top 1%"
├─ Achievements[]     ← unlocked badges ("30 Streak")
├─ Heatmap[]          ← per-day submission counts (last year)
└─ CategoryBreakdown[]← % solved per tag
```

> If streak/achievement **rules** grow complex enough to need their own invariants and
> consistency, promote `PlayerProgression` to a write-side aggregate later (this was option 3
> in the design discussion). For now it stays a projection.

---

## 5. Problems bounded context

### 5.1 `Problem` — aggregate root

Owns the authored definition **and** the hidden judging data. Single consistency boundary.

```
Problem (AggregateRoot<ProblemId>)
├─ ProblemId          : strongly-typed ID (shared kernel)
├─ Slug               : Slug (VO — immutable after creation)
├─ Title              : string
├─ Difficulty         : Difficulty (enum: Easy | Medium | Hard)
├─ Points             : int (> 0)
├─ EstimatedMinutes   : int (> 0)
├─ Tags               : IReadOnlySet<Tag>  (Tag = validated string VO)
├─ Description        : string
├─ Notes              : IReadOnlyList<string>
├─ InputFormat        : IReadOnlyList<string>
├─ Constraints        : IReadOnlyList<string>
├─ Examples           : IReadOnlyList<Example>   ← PUBLIC sample I/O (≥ 1)
│                        Example { input, output, explanation? }
├─ TestCases          : IReadOnlyList<TestCase>  ← HIDDEN, Runner only
│                        TestCase { input, expectedOutput, isHidden }
└─ IsPublished        : bool (false at creation)
```

**Invariants:**
- `Slug` is unique and URL-safe. Derived from `Title` at creation; immutable after.
- `Points > 0`.
- `EstimatedMinutes > 0`.
- At least one `Example` (enforced by `SetExamples`).
- `Publish()` requires `TestCases.Any()`.

**Factory & methods:**
- `Problem.Create(slug, title, difficulty, points, estimatedMinutes, tags, description, notes, inputFormat, constraints, examples)` — `IsPublished = false`.
- `problem.Edit(title, difficulty, points, estimatedMinutes, tags, description, notes, inputFormat, constraints)` — free edit, no domain event.
- `problem.SetExamples(IReadOnlyList<Example>)` — wholesale replace, validates ≥ 1.
- `problem.SetTestCases(IReadOnlyList<TestCase>)` — wholesale replace, no minimum.
- `problem.Publish()` — guards `TestCases.Any()`, raises `ProblemPublished`.

**Domain events emitted:** `ProblemCreated`, `ProblemPublished`.

> `ProblemEdited` is deferred — no downstream consumer exists yet. Add when a real consumer
> (e.g. search index invalidation) is introduced.

**Authorship:** Admin-only operation (enforced at Application layer). No `CreatedBy` field on the aggregate.

**NOT in Problem:** `solvedCount` (read model), `status` (per-user projection owned by Submission BC).

### 5.2 `TestSuite` — removed as separate aggregate

`TestSuite` was merged into `Problem`. Rationale: it had no invariants of its own, no separate
lifecycle, and keeping it separate only introduced a co-creation coordination problem without
DDD benefit. `TestCases` are now a collection directly on `Problem`, loaded explicitly (never
auto-included in public queries).

The gRPC method `GetTestSuite(problemId)` still exists at the API layer — it loads `Problem`
with `TestCases` via explicit include and returns the hidden cases to the Runner.

### 5.3 `solvedCount` & per-user `ProblemStatus` — read-side

- `solvedCount`: read-model counter per problem, incremented by `SolvedCountProjectionHandler`
  subscribed to `SubmissionEvaluated` (verdict = accepted). Deduped by `(userId, problemId)` —
  a `PROBLEM_FIRST_SOLVE` table (unique constraint on the pair) prevents double-counting on
  event redelivery.
- `ProblemStatus` (`start`/`continue`/`review`): a **per-(user, problem)** projection. Authority
  is the Submission context. The BFF/API composes it onto the problem list per requesting user.

---

## 6. Integration mechanics

**Reference style:** every cross-context link is an **ID only** (`UserId`, `ProblemId`).
No context stores a copy of another context's entity.

**Async (RabbitMQ) — state propagation:**

```
SubmissionEvaluatedV1 {
  submissionId, userId, problemId,
  verdict,           // accepted | wrong | error | timeout
  pointsAwarded,
  language,
  evaluatedAt
}
```
- Consumed by **Users BC** → updates `PlayerProgression`.
- Consumed by **Problems BC** → updates `solvedCount` (deduped via `PROBLEM_FIRST_SOLVE`).
- Each consumer applies an **ACL**: translate the event into its own terms; never depend on
  Submission's internal model.

**Sync (gRPC) — read-time fetch only:**
- `ProblemsService.GetProblem(slug)` — BFF/Gateway reads definition for the problem page.
- `ProblemsService.GetTestSuite(problemId)` — Submission/Runner pulls hidden cases to judge.
- `UsersService.GetProfile(userId)` — composes identity + progression for the profile page.

Rule of thumb: **events change state, gRPC answers questions.** No synchronous coordination
between Users and Problems.

---

## 7. Shared kernel (`code-sprint-shared`)

Building blocks only — **no aggregates, no business rules.** Keeping domain out of here is what
prevents the services from silently re-coupling.

```
code-sprint-shared/
├─ Primitives/      Entity<TId>, AggregateRoot, ValueObject, IDomainEvent, Result/Error
├─ Ids/             UserId, ProblemId, ... (strongly-typed ID structs)
└─ Contracts/       Integration-event DTOs (SubmissionEvaluatedV1, ...) — versioned
```

> Each BC defines its **own** domain types internally. `Difficulty` stays in the Problems BC —
> no other context needs it.

---

## 8. Remaining gaps

1. **No User domain model exists yet** — only profile UI. The `User` aggregate above is net-new.
2. **`status` leaked into Problem** — it's per-user; move composition to the BFF.
3. **No idempotency story for `PlayerProgression` handler** — `SubmissionEvaluated` can be
   redelivered; Users BC projection handler needs its own dedupe mechanism (by `submissionId`).
4. **Rank computation** — global ranking from points is expensive; decide windowed/scheduled
   recompute vs. on-read. (Deferred, but flagged.)

---

## 9. Open / deferred decisions

| Decision | Choice | Risk accepted |
|---|---|---|
| **Verdict integrity vs. editable problems** | Free edit always. | Editing test cases or constraints after submissions exist retroactively invalidates past verdicts. No invariant guards this. Revisit before problems are user-facing-stable. |
| **PlayerProgression as projection vs. aggregate** | Projection for now. | If streak/achievement rules need their own invariants, promote to write-side aggregate. |
| **`ProblemEdited` domain event** | Deferred. | No downstream consumer yet. Add with specific payload when a real consumer (search index, audit log) is introduced. |

---

## 10. Glossary

- **Problem** — an authored coding challenge: statement, constraints, scoring, public examples,
  and hidden test cases. Does *not* include any per-user state.
- **TestCase** — a hidden input/expected-output pair the Runner grades a submission against.
  Lives inside the `Problem` aggregate; never serialized to the public API.
- **Example** — a *public* sample input/output pair shown in the UI. Distinct from `TestCase`.
- **Slug** — the unique, URL-safe identifier for a Problem (e.g. `"two-sum"`). Derived from
  title at creation; immutable after.
- **Tag** — a free-form category label on a Problem (e.g. `"Strings"`). Open taxonomy;
  valid values enforced at the Application layer, not the domain.
- **User** — an identity/account: email, handle, display name, auth identities. No stats.
- **PlayerProgression** — the derived, read-only view of a user's standing (points, rank,
  streak, tier, achievements, heatmap, categories), computed from submission events.
- **ProblemStatus** — per-(user, problem) state (`start`/`continue`/`review`); owned by Submission.
- **solvedCount** — per-problem count of distinct solvers; a read-model stat.
- **SubmissionEvaluated** — the integration event published when a submission is judged; the
  single source feeding both read models.
