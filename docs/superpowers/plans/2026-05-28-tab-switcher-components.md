# TabSwitcher Components & Problem Model Extension Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace hardcoded content in `TabSwitcher` with data-driven components reading from an extended `Problem` model, matching the visual design (NoteBox, InputFormat, ConstraintsList, ExampleCard).

**Architecture:** Extend `Problem` type with 5 new fields, populate mock data, build 7 atomic components, wire them into `TabSwitcher` via a `problem` prop, and update `ProblemPage` to resolve the slug and pass the problem down.

**Tech Stack:** Next.js (app router), TypeScript, Tailwind CSS, tailwind-merge

---

## File Map

| Action | File |
|--------|------|
| Modify | `code-sprint-fe/features/problems/types.ts` |
| Modify | `code-sprint-fe/features/problems/data.ts` |
| Create | `code-sprint-fe/features/problems/components/NoteBox.tsx` |
| Create | `code-sprint-fe/features/problems/components/ConstraintsList.tsx` |
| Create | `code-sprint-fe/features/problems/components/InputFormatSection.tsx` |
| Create | `code-sprint-fe/features/problems/components/ExampleCard.tsx` |
| Create | `code-sprint-fe/features/problems/components/DescriptionTab.tsx` |
| Create | `code-sprint-fe/features/problems/components/ExamplesTab.tsx` |
| Create | `code-sprint-fe/features/problems/components/ConstraintsTab.tsx` |
| Modify | `code-sprint-fe/features/problems/components/TabSwitcher.tsx` |
| Modify | `code-sprint-fe/app/problems/[slug]/page.tsx` |

---

## Task 1: Extend types

**Files:**
- Modify: `code-sprint-fe/features/problems/types.ts`

- [ ] **Step 1: Replace the file contents**

```typescript
export type Difficulty = "easy" | "medium" | "hard";
export type FilterDifficulty = "all" | Difficulty;
export type ProblemStatus = "start" | "continue" | "review";

export type Example = {
  input: string;
  output: string;
  explanation?: string;
};

export type Problem = {
  id: string;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  solvedCount: number;
  estimatedTime: string;
  points: number;
  status: ProblemStatus;
  description: string;
  notes: string[];
  inputFormat: string[];
  constraints: string[];
  examples: Example[];
};
```

- [ ] **Step 2: Verify TypeScript sees the new types**

Run from `code-sprint-fe/`:
```bash
npx tsc --noEmit
```
Expected: errors about `data.ts` missing the new fields (confirms types are picked up). No unrelated errors.

---

## Task 2: Update mock data

**Files:**
- Modify: `code-sprint-fe/features/problems/data.ts`

- [ ] **Step 1: Replace the file with extended mock entries**

```typescript
import type { Problem } from "./types";

export const problems: Problem[] = [
  {
    id: "1",
    title: "Two Sum",
    difficulty: "easy",
    tags: ["Mathematical"],
    solvedCount: 12400,
    estimatedTime: "10 min",
    points: 50,
    status: "review",
    description:
      "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target. You may assume that each input would have exactly one solution, and you may not use the same element twice.",
    notes: ["Each input has exactly one solution.", "You may not use the same element twice."],
    inputFormat: [
      "First line: integer N (size of array)",
      "Second line: N space-separated integers",
      "Third line: integer target",
    ],
    constraints: ["2 ≤ N ≤ 10^4", "-10^9 ≤ nums[i] ≤ 10^9", "-10^9 ≤ target ≤ 10^9"],
    examples: [
      {
        input: "nums = [2, 7, 11, 15]\ntarget = 9",
        output: "[0, 1]",
        explanation: "nums[0] + nums[1] = 2 + 7 = 9",
      },
      {
        input: "nums = [3, 2, 4]\ntarget = 6",
        output: "[1, 2]",
      },
    ],
  },
  {
    id: "2",
    title: "Valid Parentheses",
    difficulty: "easy",
    tags: ["Strings"],
    solvedCount: 9800,
    estimatedTime: "15 min",
    points: 50,
    status: "continue",
    description:
      "Given a string s containing just the characters '(', ')', '{', '}', '[' and ']', determine if the input string is valid. An input string is valid if open brackets are closed by the same type of brackets, and in the correct order.",
    notes: ["Empty string is considered valid."],
    inputFormat: ["Single line: string s"],
    constraints: ["1 ≤ s.length ≤ 10^4", "s consists of parentheses only '()[]{}'"],
    examples: [
      {
        input: 's = "()"',
        output: "true",
      },
      {
        input: 's = "()[]{}"',
        output: "true",
      },
      {
        input: 's = "(]"',
        output: "false",
        explanation: "Mismatched bracket types.",
      },
    ],
  },
  {
    id: "3",
    title: "Longest Substring Without Repeating Characters",
    difficulty: "medium",
    tags: ["Strings"],
    solvedCount: 7200,
    estimatedTime: "20 min",
    points: 100,
    status: "start",
    description:
      "Given a string s, find the length of the longest substring without repeating characters.",
    notes: ["A substring is a contiguous non-empty sequence of characters within a string."],
    inputFormat: ["Single line: string s"],
    constraints: ["0 ≤ s.length ≤ 5 × 10^4", "s consists of English letters, digits, symbols and spaces"],
    examples: [
      {
        input: 's = "abcabcbb"',
        output: "3",
        explanation: 'The answer is "abc", with the length of 3.',
      },
      {
        input: 's = "bbbbb"',
        output: "1",
        explanation: 'The answer is "b", with the length of 1.',
      },
    ],
  },
  {
    id: "4",
    title: "3Sum",
    difficulty: "medium",
    tags: ["Mathematical"],
    solvedCount: 5100,
    estimatedTime: "25 min",
    points: 100,
    status: "start",
    description:
      "Given an integer array nums, return all the triplets [nums[i], nums[j], nums[k]] such that i != j, i != k, j != k, and nums[i] + nums[j] + nums[k] == 0. The solution set must not contain duplicate triplets.",
    notes: ["Solution set must not contain duplicate triplets."],
    inputFormat: ["First line: integer N", "Second line: N space-separated integers"],
    constraints: ["3 ≤ N ≤ 3000", "-10^5 ≤ nums[i] ≤ 10^5"],
    examples: [
      {
        input: "nums = [-1, 0, 1, 2, -1, -4]",
        output: "[[-1,-1,2],[-1,0,1]]",
        explanation: "Two unique triplets sum to zero.",
      },
    ],
  },
  {
    id: "5",
    title: "Median of Two Sorted Arrays",
    difficulty: "hard",
    tags: ["Logical"],
    solvedCount: 2300,
    estimatedTime: "40 min",
    points: 200,
    status: "start",
    description:
      "Given two sorted arrays nums1 and nums2 of size m and n respectively, return the median of the two sorted arrays. The overall run time complexity should be O(log(m+n)).",
    notes: ["Required time complexity: O(log(m+n))."],
    inputFormat: [
      "First line: integer m",
      "Second line: m sorted integers (nums1)",
      "Third line: integer n",
      "Fourth line: n sorted integers (nums2)",
    ],
    constraints: ["0 ≤ m, n ≤ 1000", "1 ≤ m + n ≤ 2000", "-10^6 ≤ nums1[i], nums2[i] ≤ 10^6"],
    examples: [
      {
        input: "nums1 = [1, 3]\nnums2 = [2]",
        output: "2.00000",
        explanation: "Merged array = [1,2,3], median is 2.",
      },
    ],
  },
  {
    id: "6",
    title: "Trapping Rain Water",
    difficulty: "hard",
    tags: ["Geometrics"],
    solvedCount: 1900,
    estimatedTime: "35 min",
    points: 200,
    status: "start",
    description:
      "Given n non-negative integers representing an elevation map where the width of each bar is 1, compute how much water it can trap after raining.",
    notes: ["Each bar has width 1."],
    inputFormat: ["First line: integer n", "Second line: n space-separated non-negative integers"],
    constraints: ["1 ≤ n ≤ 2 × 10^4", "0 ≤ height[i] ≤ 10^5"],
    examples: [
      {
        input: "height = [0, 1, 0, 2, 1, 0, 1, 3, 2, 1, 2, 1]",
        output: "6",
        explanation: "6 units of rain water are trapped.",
      },
    ],
  },
  {
    id: "7",
    title: "Binary Search",
    difficulty: "easy",
    tags: ["Logical"],
    solvedCount: 11000,
    estimatedTime: "10 min",
    points: 50,
    status: "review",
    description:
      "Given an array of integers nums which is sorted in ascending order, and an integer target, write a function to search target in nums. If target exists, return its index. Otherwise, return -1. You must write an algorithm with O(log n) runtime complexity.",
    notes: ["Array is sorted in ascending order.", "Required time complexity: O(log n)."],
    inputFormat: [
      "First line: integer n",
      "Second line: n sorted integers",
      "Third line: integer target",
    ],
    constraints: ["1 ≤ n ≤ 10^4", "-10^4 ≤ nums[i], target ≤ 10^4", "All integers in nums are unique"],
    examples: [
      {
        input: "nums = [-1, 0, 3, 5, 9, 12]\ntarget = 9",
        output: "4",
        explanation: "9 exists in nums and its index is 4.",
      },
      {
        input: "nums = [-1, 0, 3, 5, 9, 12]\ntarget = 2",
        output: "-1",
        explanation: "2 does not exist in nums so return -1.",
      },
    ],
  },
  {
    id: "8",
    title: "Merge Intervals",
    difficulty: "medium",
    tags: ["Mathematical"],
    solvedCount: 6500,
    estimatedTime: "20 min",
    points: 100,
    status: "start",
    description:
      "Given an array of intervals where intervals[i] = [start_i, end_i], merge all overlapping intervals, and return an array of the non-overlapping intervals that cover all the intervals in the input.",
    notes: ["Intervals may be given in any order."],
    inputFormat: [
      "First line: integer n (number of intervals)",
      "Next n lines: two integers start_i end_i",
    ],
    constraints: ["1 ≤ n ≤ 10^4", "0 ≤ start_i ≤ end_i ≤ 10^4"],
    examples: [
      {
        input: "intervals = [[1,3],[2,6],[8,10],[15,18]]",
        output: "[[1,6],[8,10],[15,18]]",
        explanation: "[1,3] and [2,6] overlap so merge to [1,6].",
      },
    ],
  },
  {
    id: "9",
    title: "Word Search",
    difficulty: "medium",
    tags: ["Strings", "Logical"],
    solvedCount: 4800,
    estimatedTime: "30 min",
    points: 100,
    status: "start",
    description:
      "Given an m x n grid of characters board and a string word, return true if word exists in the grid. The word can be constructed from letters of sequentially adjacent cells (horizontally or vertically). The same letter cell may not be used more than once.",
    notes: ["Same cell may not be used more than once."],
    inputFormat: [
      "First line: integers m n",
      "Next m lines: n characters each (the grid)",
      "Last line: string word",
    ],
    constraints: ["1 ≤ m, n ≤ 6", "1 ≤ word.length ≤ 15", "board and word consist only of lowercase and uppercase English letters"],
    examples: [
      {
        input: 'board = [["A","B","C","E"],["S","F","C","S"],["A","D","E","E"]]\nword = "ABCCED"',
        output: "true",
      },
    ],
  },
  {
    id: "10",
    title: "Minimum Window Substring",
    difficulty: "hard",
    tags: ["Strings"],
    solvedCount: 1200,
    estimatedTime: "45 min",
    points: 200,
    status: "start",
    description:
      "Given two strings s and t of lengths m and n respectively, return the minimum window substring of s such that every character in t (including duplicates) is included in the window. If there is no such substring, return the empty string.",
    notes: [
      "The answer is guaranteed to be unique.",
      "If no valid window exists, return empty string.",
    ],
    inputFormat: ["First line: string s", "Second line: string t"],
    constraints: ["1 ≤ m, n ≤ 10^5", "s and t consist of uppercase and lowercase English letters"],
    examples: [
      {
        input: 's = "ADOBECODEBANC"\nt = "ABC"',
        output: '"BANC"',
        explanation: "Minimum window containing A, B, C is BANC.",
      },
    ],
  },
];
```

- [ ] **Step 2: Verify types pass**

Run from `code-sprint-fe/`:
```bash
npx tsc --noEmit
```
Expected: errors in `TabSwitcher.tsx` (still no `problem` prop) but `data.ts` and `types.ts` clean.

- [ ] **Step 3: Commit**

```bash
git add code-sprint-fe/features/problems/types.ts code-sprint-fe/features/problems/data.ts
git commit -m "feat: extend Problem type with description, notes, inputFormat, constraints, examples"
```

---

## Task 3: NoteBox component

**Files:**
- Create: `code-sprint-fe/features/problems/components/NoteBox.tsx`

- [ ] **Step 1: Create the component**

```tsx
type NoteBoxProps = {
  content: string;
};

export function NoteBox({ content }: NoteBoxProps) {
  return (
    <div className="mt-2 rounded-xl border-2 border-primary-500 bg-primary-100 p-2">
      <h3 className="font-black leading-tight text-primary-500">Note</h3>
      <span>{content}</span>
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/NoteBox.tsx
git commit -m "feat: add NoteBox component"
```

---

## Task 4: ConstraintsList component

**Files:**
- Create: `code-sprint-fe/features/problems/components/ConstraintsList.tsx`

- [ ] **Step 1: Create the component**

```tsx
type ConstraintsListProps = {
  items: string[];
};

export function ConstraintsList({ items }: ConstraintsListProps) {
  return (
    <ul className="flex flex-col gap-1">
      {items.map((item, i) => (
        <li key={i} className="flex items-center gap-2">
          <span className="h-2 w-2 shrink-0 rounded-full bg-primary-500" />
          <span>{item}</span>
        </li>
      ))}
    </ul>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/ConstraintsList.tsx
git commit -m "feat: add ConstraintsList component"
```

---

## Task 5: InputFormatSection component

**Files:**
- Create: `code-sprint-fe/features/problems/components/InputFormatSection.tsx`

- [ ] **Step 1: Create the component**

```tsx
type InputFormatSectionProps = {
  lines: string[];
};

export function InputFormatSection({ lines }: InputFormatSectionProps) {
  return (
    <div className="flex flex-col gap-1">
      <h3 className="font-bold">Input Format</h3>
      {lines.map((line, i) => (
        <p key={i} className="text-sm text-neutral-700">{line}</p>
      ))}
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/InputFormatSection.tsx
git commit -m "feat: add InputFormatSection component"
```

---

## Task 6: ExampleCard component

**Files:**
- Create: `code-sprint-fe/features/problems/components/ExampleCard.tsx`

- [ ] **Step 1: Create the component**

```tsx
import type { Example } from "../types";

type ExampleCardProps = {
  example: Example;
  index: number;
};

export function ExampleCard({ example, index }: ExampleCardProps) {
  return (
    <div className="flex flex-col gap-2 rounded-xl border border-neutral-200 bg-white p-4">
      <h3 className="font-bold">Example {index}</h3>
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold text-neutral-500">Input</span>
        <pre className="rounded-lg bg-neutral-100 p-2 text-sm whitespace-pre-wrap">{example.input}</pre>
      </div>
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold text-neutral-500">Output</span>
        <pre className="rounded-lg bg-neutral-100 p-2 text-sm whitespace-pre-wrap">{example.output}</pre>
      </div>
      {example.explanation && (
        <div className="flex flex-col gap-1">
          <span className="text-xs font-semibold text-neutral-500">Explanation</span>
          <p className="text-sm text-neutral-700">{example.explanation}</p>
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/ExampleCard.tsx
git commit -m "feat: add ExampleCard component"
```

---

## Task 7: DescriptionTab component

**Files:**
- Create: `code-sprint-fe/features/problems/components/DescriptionTab.tsx`

- [ ] **Step 1: Create the component**

```tsx
import type { Problem } from "../types";
import { NoteBox } from "./NoteBox";

type DescriptionTabProps = {
  problem: Problem;
};

export function DescriptionTab({ problem }: DescriptionTabProps) {
  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm leading-relaxed text-neutral-700">{problem.description}</p>
      {problem.notes.map((note, i) => (
        <NoteBox key={i} content={note} />
      ))}
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/DescriptionTab.tsx
git commit -m "feat: add DescriptionTab component"
```

---

## Task 8: ExamplesTab component

**Files:**
- Create: `code-sprint-fe/features/problems/components/ExamplesTab.tsx`

- [ ] **Step 1: Create the component**

```tsx
import type { Problem } from "../types";
import { ExampleCard } from "./ExampleCard";

type ExamplesTabProps = {
  problem: Problem;
};

export function ExamplesTab({ problem }: ExamplesTabProps) {
  return (
    <div className="flex flex-col gap-4">
      {problem.examples.map((example, i) => (
        <ExampleCard key={i} example={example} index={i + 1} />
      ))}
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/ExamplesTab.tsx
git commit -m "feat: add ExamplesTab component"
```

---

## Task 9: ConstraintsTab component

**Files:**
- Create: `code-sprint-fe/features/problems/components/ConstraintsTab.tsx`

- [ ] **Step 1: Create the component**

```tsx
import type { Problem } from "../types";
import { ConstraintsList } from "./ConstraintsList";
import { InputFormatSection } from "./InputFormatSection";

type ConstraintsTabProps = {
  problem: Problem;
};

export function ConstraintsTab({ problem }: ConstraintsTabProps) {
  return (
    <div className="flex flex-col gap-6">
      <InputFormatSection lines={problem.inputFormat} />
      <div className="flex flex-col gap-2">
        <h3 className="font-bold">Constraints</h3>
        <ConstraintsList items={problem.constraints} />
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add code-sprint-fe/features/problems/components/ConstraintsTab.tsx
git commit -m "feat: add ConstraintsTab component"
```

---

## Task 10: Update TabSwitcher

**Files:**
- Modify: `code-sprint-fe/features/problems/components/TabSwitcher.tsx`

- [ ] **Step 1: Replace the file**

```tsx
"use client";

import { useState } from "react";
import type { Problem } from "../types";
import { ConstraintsTab } from "./ConstraintsTab";
import { DescriptionTab } from "./DescriptionTab";
import { ExamplesTab } from "./ExamplesTab";

const tabs = ["Description", "Examples", "Constraints"] as const;
type TabType = (typeof tabs)[number];

type TabSwitcherProps = {
  problem: Problem;
};

export function TabSwitcher({ problem }: TabSwitcherProps) {
  const [currentTab, setTab] = useState<TabType>("Description");

  return (
    <div className="mt-4 inline-flex w-full flex-col">
      <div className="flex">
        {tabs.map((tab) => (
          <button
            key={tab}
            onClick={() => setTab(tab)}
            className={
              currentTab === tab
                ? "-mb-px flex-1 rounded-t-md border border-neutral-300 border-b-white bg-white px-4 pb-4 font-semibold"
                : "flex-1 px-4 pb-4 text-neutral-500"
            }
          >
            {tab}
          </button>
        ))}
      </div>
      <div className="rounded-b-md rounded-tr-md border border-neutral-300 bg-white p-4">
        {currentTab === "Description" && <DescriptionTab problem={problem} />}
        {currentTab === "Examples" && <ExamplesTab problem={problem} />}
        {currentTab === "Constraints" && <ConstraintsTab problem={problem} />}
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Run type check**

Run from `code-sprint-fe/`:
```bash
npx tsc --noEmit
```
Expected: only error is in `app/problems/[slug]/page.tsx` (TabSwitcher called without `problem` prop).

- [ ] **Step 3: Commit**

```bash
git add code-sprint-fe/features/problems/components/TabSwitcher.tsx
git commit -m "feat: wire TabSwitcher to Problem prop, fix Contraints typo"
```

---

## Task 11: Update ProblemPage

**Files:**
- Modify: `code-sprint-fe/app/problems/[slug]/page.tsx`

The page currently has all content hardcoded. Update it to resolve the slug against mock data and pass the problem to `TabSwitcher`. Slug is matched against `problem.id`.

- [ ] **Step 1: Replace the file**

```tsx
import { Badge } from "@/components/ui/Badge";
import { Container } from "@/components/ui/Container";
import { StatItem } from "@/components/ui/StatItem";
import { problems } from "@/features/problems/data";
import { StartSolving } from "@/features/problems/components/StartSolving";
import { TabSwitcher } from "@/features/problems/components/TabSwitcher";
import { notFound } from "next/navigation";

export default async function ProblemPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const problem = problems.find((p) => p.id === slug);

  if (!problem) {
    notFound();
  }

  const difficultyVariant = {
    easy: "green",
    medium: "yellow",
    hard: "red",
  } as const;

  return (
    <div className="flex flex-col gap-5 p-4">
      <Container>
        <div className="flex flex-row justify-between">
          <div className="flex flex-row gap-2">
            <Badge variant={difficultyVariant[problem.difficulty]}>
              {problem.difficulty.charAt(0).toUpperCase() + problem.difficulty.slice(1)}
            </Badge>
            <Badge variant="red">{problem.points} pts</Badge>
          </div>
          <Badge variant={problem.status === "review" ? "green" : "yellow"}>
            {problem.status === "review" ? "Solved" : problem.status === "continue" ? "In Progress" : "Not Started"}
          </Badge>
        </div>
        <h2 className="text-2xl leading-tight font-black">{problem.title}</h2>
        <p>Problem ID: #{problem.id} | ~{problem.estimatedTime}</p>
        <hr className="border-neutral-200" />
        <div className="flex flex-row justify-between">
          <StatItem value={problem.solvedCount.toLocaleString()} label="Solved" />
          <StatItem value={String(problem.points)} label="Points" />
        </div>
      </Container>
      <TabSwitcher problem={problem} />
      <StartSolving />
    </div>
  );
}
```

- [ ] **Step 2: Verify type check passes clean**

Run from `code-sprint-fe/`:
```bash
npx tsc --noEmit
```
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add code-sprint-fe/app/problems/[slug]/page.tsx
git commit -m "feat: resolve problem by slug and pass to TabSwitcher"
```

---

## Task 12: Build verification

- [ ] **Step 1: Run Next.js build**

Run from `code-sprint-fe/`:
```bash
pnpm run build
```
Expected: build completes with no TypeScript errors. Warnings about static generation for `[slug]` page are acceptable.
