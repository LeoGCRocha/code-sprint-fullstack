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
  ┌───────────────┐   gRPC (read TestSuite)    ┌──────────────────┐
  │  Submission   │ ─────────────────────────▶ │   Problems BC    │
  │   (upstream   │                            │  Problem (AR)    │
  │   publisher)  │                            │  TestSuite (AR)  │
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
- **Submission → Problems** (gRPC TestSuite fetch): *Customer–Supplier*; Problems is the
  supplier of the judging data.
- **Users ↔ Problems**: **no relationship.** They share no schema, no object model, no calls.
  Their only connection is indirect, through Submission events.

---

## 3. The layers (per bounded context)

Each service is a vertical slice with the standard four layers. Dependencies point **inward**
(Domain depends on nothing).

| Layer | Responsibility | Examples (Users BC) | Examples (Problems BC) |
|---|---|---|---|
| **Domain** | Aggregates, entities, value objects, domain events, invariants. Pure, no I/O. | `User`, `Handle`, `Email`, `UserRegistered` | `Problem`, `TestSuite`, `Slug`, `Difficulty`, `ScorePoints`, `Example` |
| **Application** | Use cases / command + query handlers, orchestration, transaction boundary, ports (interfaces). | `RegisterUser`, `ChangeHandle`, `GetProfile` | `CreateProblem`, `EditProblem`, `GetProblem`, `ListProblems` |
| **Infrastructure** | Adapters: EF Core repos, Postgres, RabbitMQ publish/subscribe, gRPC servers/clients, OAuth. | `UserRepository`, `ProgressionProjectionHandler` | `ProblemRepository`, `TestSuiteRepository`, `ProblemGrpcService`, `SolvedCountProjectionHandler` |
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
> in the design discussion). For now it stays a projection — see §7.

---

## 5. Problems bounded context

### 5.1 `Problem` — aggregate root

Owns only the authored definition.

```
Problem (AggregateRoot)
├─ ProblemId      : value object (typed ID)
├─ Slug           : value object (unique, URL-safe)
├─ Title          : value object
├─ Difficulty     : enum value object (easy|medium|hard)
├─ ScorePoints    : value object (> 0)
├─ Tags           : set<Tag>
├─ EstimatedTime  : value object
├─ Statement      : value object { description, notes[], inputFormat[], constraints[] }
└─ Examples       : list<Example>   ← PUBLIC sample I/O (value objects)
                     Example { input, output, explanation? }
```

**Invariants:**
- `Slug` is unique and URL-safe.
- `ScorePoints > 0`.
- At least one `Example`.

**Domain events:** `ProblemCreated`, `ProblemEdited`.

### 5.2 `TestSuite` — separate aggregate (same BC)

Hidden judging data. Kept out of `Problem` because it is large, hidden, and changes on a
different cadence — bloating the Problem aggregate with it would couple authoring to test data.

```
TestSuite (AggregateRoot, linked by ProblemId)
├─ TestSuiteId : value object
├─ ProblemId   : foreign reference (same BC)
└─ Cases       : list<TestCase>
                  TestCase { input, expectedOutput, weight?, isHidden }
```

- Loaded **only** by the Runner/Submission flow via gRPC (`GetTestSuite(problemId)`).
- Never serialized to the public API.

### 5.3 `solvedCount` & per-user `ProblemStatus` — read-side

- `solvedCount`: read-model counter per problem, incremented by a handler subscribed to
  `SubmissionEvaluated` (verdict = accepted, first solve by that user). Never on the write model.
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
- Consumed by **Problems BC** → updates `solvedCount`.
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

> Each BC defines its **own** `User` / `Problem` domain types internally. If `Difficulty` or
> another enum ever needs to be shared, that's the *only* kind of domain concept allowed up here
> — and only if no context wants to diverge.

---

## 8. What you're missing / gaps to close

1. **No User domain model exists yet** — only profile UI. The `User` aggregate above is net-new.
2. **`solvedCount` was on the write model** — make it a projection or it will lie/contend.
3. **`status` leaked into Problem** — it's per-user; move composition to the BFF.
4. **No `TestSuite`** — the Runner has nothing to judge against; only public `examples` exist.
5. **No idempotency story for event consumers** — `SubmissionEvaluated` can be redelivered;
   projection handlers must dedupe (e.g. by `submissionId`).
6. **Rank computation** — global ranking from points is expensive; decide windowed/scheduled
   recompute vs. on-read. (Deferred, but flagged.)
7. **Verdict integrity vs. editable problems** — see §9.

---

## 9. Open / deferred decisions

| Decision | Choice | Risk accepted |
|---|---|---|
| **Problem lifecycle / versioning** | **Deferred.** Problems are always-published and freely editable for now. | Editing constraints or test cases after submissions exist retroactively invalidates past verdicts. No invariant guards this yet. Revisit before launch / before problems are user-facing-stable. |
| **PlayerProgression as projection vs. aggregate** | Projection for now. | If streak/achievement rules need their own invariants, promote to write-side aggregate. |

---

## 10. Glossary

- **Problem** — an authored coding challenge: statement, constraints, scoring, public examples.
  Does *not* include hidden test data or any per-user state.
- **TestSuite** — the hidden set of `TestCase`s the Runner grades a submission against.
- **Example** — a *public* sample input/output pair shown in the UI.
- **User** — an identity/account: email, handle, display name, auth identities. No stats.
- **PlayerProgression** — the derived, read-only view of a user's standing (points, rank,
  streak, tier, achievements, heatmap, categories), computed from submission events.
- **ProblemStatus** — per-(user, problem) state (`start`/`continue`/`review`); owned by Submission.
- **solvedCount** — per-problem count of distinct solvers; a read-model stat.
- **SubmissionEvaluated** — the integration event published when a submission is judged; the
  single source feeding both read models.
