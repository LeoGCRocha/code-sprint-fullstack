# VSA Refactor + Deduplication Design

**Date:** 2026-05-26  
**Scope:** `code-sprint-fe/` — restructure to Vertical Slice Architecture, eliminate duplication  
**Status:** Approved

---

## 1. Architecture

Organize by feature slice, not by technical layer. Each slice owns its components, types, and data. Shared primitives live in `components/ui/`. Layout shell lives in `components/layout/`.

```
code-sprint-fe/
├── app/
│   ├── globals.css
│   ├── layout.tsx              ← unchanged
│   ├── page.tsx                ← thin: imports from features/home
│   ├── login/
│   │   └── page.tsx            ← thin: imports from features/auth
│   └── problems/
│       └── page.tsx            ← thin: imports from features/problems
│
├── features/
│   ├── home/
│   │   └── components/
│   │       ├── Hero.tsx
│   │       ├── PracticeModeCard.tsx      ← extracted from app/page.tsx
│   │       └── CompetitionModeCard.tsx   ← extracted from app/page.tsx
│   ├── auth/
│   │   └── components/
│   │       └── LoginCard.tsx             ← extracted from login/page.tsx
│   └── problems/
│       ├── types.ts            ← Problem, Difficulty, FilterDifficulty
│       ├── data.ts             ← mock problems array
│       └── components/
│           ├── ProblemList.tsx
│           ├── ProblemCard.tsx           ← extracted from ProblemList.map
│           ├── ProblemFilter.tsx         ← renamed from ProblemSelection
│           └── UserRank.tsx
│
└── components/
    ├── ui/
    │   ├── Button.tsx
    │   ├── Badge.tsx
    │   ├── Pagination.tsx                ← extracted from ProblemList
    │   └── StatItem.tsx                  ← promoted from private fn in UserRank
    └── layout/
        ├── Navbar.tsx
        ├── Footer.tsx
        ├── Logo.tsx
        └── TypeWriter.tsx
```

**Boundary rule:** features may import from `components/ui/` and `components/layout/`. Features must not import from other features.

---

## 2. Duplication Fixes

| # | Problem | Fix |
|---|---------|-----|
| 1 | `Difficulty` type defined in `Badge` (via `Problem["difficulty"]`) AND independently in `ProblemSelection` | Canonical def in `features/problems/types.ts`; `Badge` defines its own inline `"easy" \| "medium" \| "hard"` to stay feature-agnostic |
| 2 | `StatItem` private fn in `UserRank` duplicates inline stat card pattern in `app/page.tsx` | Promote to `components/ui/StatItem.tsx`; use in `UserRank` and `CompetitionModeCard` |
| 3 | Pagination logic + raw `<button>` elements embedded in `ProblemList` | Extract to `components/ui/Pagination.tsx` accepting `page`, `totalPages`, `onPageChange` |
| 4 | `ProblemCard` markup anonymous inside `ProblemList.map` | Extract to `features/problems/components/ProblemCard.tsx` |
| 5 | Raw `<button>` in `ProblemList`, `<p>` used as button in `ProblemFilter` | Replace with `Button` component from `components/ui/` |
| 6 | Mock data co-located with type def in `model/Problem.ts` | `Problem` type → `features/problems/types.ts`; data → `features/problems/data.ts`; delete `model/` |

---

## 3. New Generic Components

### `components/ui/Pagination.tsx`
```ts
interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}
```
Encapsulates `getPageRange` logic + prev/next buttons + ellipsis rendering.

### `components/ui/StatItem.tsx`
```ts
interface StatItemProps {
  value: string;
  label: string;
}
```
Extracted from `UserRank`'s private `StatItem`. Used in `UserRank` and `CompetitionModeCard`.

---

## 4. Data Flow

- `app/page.tsx` → renders `<PracticeModeCard />` + `<CompetitionModeCard />`
- `app/login/page.tsx` → renders `<LoginCard />`
- `app/problems/page.tsx` → renders `<ProblemFilter />`, `<UserRank />`, `<ProblemList />`
- `ProblemList` → renders `<ProblemCard />` per item, `<Pagination />` below
- `ProblemFilter` manages local `FilterDifficulty` state; future: lifted to URL params
- All problems data read from `features/problems/data.ts` (future: TanStack Query)

---

## 5. Out of Scope

- Backend integration / TanStack Query (noted in TODO comments, not addressed here)
- URL-param-driven filtering
- Testing
- Competition slice (no existing code)
