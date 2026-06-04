# ER Mapping — Users & Problems

> Physical (persistence) view of [`domain-model.md`](./domain-model.md). Shows how the
> aggregates and read models land in Postgres.
>
> **DDD rule made visible:** no relationship line crosses a bounded context. Cross-context
> links (`user_id`, `problem_id`) are stored as bare values — joined at the BFF, **never** by
> SQL foreign key. That is the "schema boundaries" arrow in the architecture diagram.

```mermaid
erDiagram
%% ============ USERS BC (schema: users) ============
    USER ||--o{ AUTH_IDENTITY : "has"
    USER ||--o| PLAYER_PROGRESSION : "projected as"
    PLAYER_PROGRESSION ||--o{ ACHIEVEMENT : "unlocked"
    PLAYER_PROGRESSION ||--o{ HEATMAP_DAY : "activity"
    PLAYER_PROGRESSION ||--o{ CATEGORY_STAT : "breakdown"

    USER {
        uuid user_id PK
        string email UK "validated"
        string handle UK "@johndoe"
        string display_name
        string avatar "initials|url"
        timestamptz member_since
    }
    AUTH_IDENTITY {
        uuid identity_id PK
        uuid user_id FK
        string provider "google|github|local"
        string provider_subject UK
    }
    PLAYER_PROGRESSION {
        uuid user_id PK "= USER.user_id (same BC)"
        int total_points "READ MODEL"
        int rank
        int current_streak
        string tier "Expert"
        string percentile "Top 1%"
        timestamptz updated_at
    }
    ACHIEVEMENT {
        uuid user_id FK
        string code PK "30_STREAK"
        timestamptz earned_at
    }
    HEATMAP_DAY {
        uuid user_id FK
        date day PK
        int count
    }
    CATEGORY_STAT {
        uuid user_id FK
        string tag PK
        int percent
    }

%% ============ PROBLEMS BC (schema: problems) ============
    PROBLEM ||--o{ EXAMPLE : "embeds (VO)"
    PROBLEM ||--o{ PROBLEM_TAG : "tagged"
    PROBLEM ||--o| TEST_SUITE : "judged by"
    PROBLEM ||--o| PROBLEM_SOLVE_STAT : "projected as"
    TEST_SUITE ||--o{ TEST_CASE : "contains"

    PROBLEM {
        uuid problem_id PK
        string slug UK "url-safe"
        string title
        string difficulty "easy|medium|hard"
        int score_points "> 0"
        string estimated_time
        text description
        jsonb notes "string[]"
        jsonb input_format "string[]"
        jsonb constraints "string[]"
    }
    EXAMPLE {
        uuid problem_id FK
        int ordinal PK
        text input
        text output
        text explanation "nullable"
    }
    PROBLEM_TAG {
        uuid problem_id FK
        string tag PK
    }
    TEST_SUITE {
        uuid test_suite_id PK
        uuid problem_id FK,UK "1:1"
    }
    TEST_CASE {
        uuid test_suite_id FK
        int ordinal PK
        text input "HIDDEN"
        text expected_output "HIDDEN"
        int weight
        bool is_hidden
    }
    PROBLEM_SOLVE_STAT {
        uuid problem_id PK "= PROBLEM.problem_id (same BC)"
        int solved_count "READ MODEL, event-fed"
    }

%% ============ SUBMISSION BC (out of scope, shown for refs) ============
    USER_PROBLEM_STATUS {
        uuid user_id "FK-by-id -> users.USER (NO db FK)"
        uuid problem_id "FK-by-id -> problems.PROBLEM (NO db FK)"
        string status "start|continue|review"
    }
```

## Reading notes

- **No line crosses a schema boundary.** `USER_PROBLEM_STATUS`, `total_points`, `solved_count`
  carry `user_id`/`problem_id` as bare values — joined at the BFF, never by SQL FK.
- **Solid lines = real DB FKs**, only inside one schema (one BC, one Postgres schema).
- **`EXAMPLE` / `PROBLEM_TAG`** — value objects in the domain, but child tables physically
  (collections can't be a single column). `notes` / `input_format` / `constraints` stay `jsonb`
  (ordered string lists, never queried individually).
- **Read models** (`PLAYER_PROGRESSION` + children, `PROBLEM_SOLVE_STAT`) — fed by
  `SubmissionEvaluated`. Rebuildable, not authoritative.
- **`USER_PROBLEM_STATUS`** — Submission BC, out of scope. Shown so the `status` field's true
  home is visible.
