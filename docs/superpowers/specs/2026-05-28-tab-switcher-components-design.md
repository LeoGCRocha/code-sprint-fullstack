# TabSwitcher Components & Problem Model Extension

**Date:** 2026-05-28  
**Scope:** `code-sprint-fe/features/problems`

## Goal

Replace hardcoded content in `TabSwitcher` with data-driven components that read from an extended `Problem` model. Match the visual design: Note callout box, Input Format section, Constraints bullet list, Example cards.

---

## Data Model

### New types in `types.ts`

```typescript
export type Example = {
  input: string;
  output: string;
  explanation?: string;
};
```

### Extended `Problem` type

Add to existing `Problem`:

```typescript
description: string;
notes: string[];
inputFormat: string[];
constraints: string[];
examples: Example[];
```

Existing fields (`id`, `title`, `difficulty`, `tags`, `solvedCount`, `estimatedTime`, `points`, `status`) unchanged.

### Mock data update

All entries in `data.ts` get the five new fields populated with realistic mock values matching each problem's domain.

---

## Components

All files in `features/problems/components/`.

### `NoteBox`

```tsx
NoteBox({ content: string })
```

Orange-bordered callout. Primary-100 background, primary-500 border. Bold "Note" heading. Matches existing inline style in TabSwitcher.

### `ConstraintsList`

```tsx
ConstraintsList({ items: string[] })
```

Unordered list. Each item prefixed with orange (primary) bullet dot. No "Constraints" heading — callers provide headings.

### `InputFormatSection`

```tsx
InputFormatSection({ lines: string[] })
```

"Input Format" bold heading. Each line rendered as plain text paragraph. Wraps in a card/section container.

### `ExampleCard`

```tsx
ExampleCard({ example: Example, index: number })
```

Displays "Example N" heading. Input and output in separate labeled code blocks. Optional explanation rendered as plain text below.

### `DescriptionTab`

```tsx
DescriptionTab({ problem: Problem })
```

Renders `problem.description` as prose, followed by one `NoteBox` per entry in `problem.notes`.

### `ExamplesTab`

```tsx
ExamplesTab({ problem: Problem })
```

Maps `problem.examples` to `ExampleCard` components, 1-indexed.

### `ConstraintsTab`

```tsx
ConstraintsTab({ problem: Problem })
```

Renders `InputFormatSection` with `problem.inputFormat`, then a "Constraints" heading, then `ConstraintsList` with `problem.constraints`.

---

## TabSwitcher Changes

### Interface

```tsx
// before
export function TabSwitcher()

// after
export function TabSwitcher({ problem }: { problem: Problem })
```

### Behavior

- Fix typo: `"Contraints"` → `"Constraints"` (tab label and type union)
- Switch renders correct tab component based on `currentTab`:
  - `"Description"` → `<DescriptionTab problem={problem} />`
  - `"Examples"` → `<ExamplesTab problem={problem} />`
  - `"Constraints"` → `<ConstraintsTab problem={problem} />`

### Caller

Wherever `TabSwitcher` is mounted must pass a `Problem` prop down. No data fetching inside `TabSwitcher`.

---

## File Checklist

- `features/problems/types.ts` — add `Example` type, extend `Problem`
- `features/problems/data.ts` — populate new fields in all mock entries
- `features/problems/components/NoteBox.tsx` — new
- `features/problems/components/ConstraintsList.tsx` — new
- `features/problems/components/InputFormatSection.tsx` — new
- `features/problems/components/ExampleCard.tsx` — new
- `features/problems/components/DescriptionTab.tsx` — new
- `features/problems/components/ExamplesTab.tsx` — new
- `features/problems/components/ConstraintsTab.tsx` — new
- `features/problems/components/TabSwitcher.tsx` — update props + switch logic
