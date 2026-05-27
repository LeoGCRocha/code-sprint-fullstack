# VSA Refactor + Deduplication Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure `code-sprint-fe/` from a flat `components/` layout to Vertical Slice Architecture, extracting generic UI primitives and eliminating code duplication.

**Architecture:** Feature slices under `features/<slice>/components/`, shared primitives under `components/ui/`, layout shell under `components/layout/`. App pages become thin wrappers importing from feature slices. No cross-feature imports.

**Tech Stack:** Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, tailwind-merge

---

## File Map

### New files to create

| File | Responsibility |
|------|----------------|
| `features/problems/types.ts` | `Problem`, `Difficulty`, `FilterDifficulty` types |
| `features/problems/data.ts` | Mock problems array |
| `features/problems/components/ProblemCard.tsx` | Single problem row |
| `features/problems/components/ProblemFilter.tsx` | Difficulty filter pills |
| `features/problems/components/UserRank.tsx` | User progress panel |
| `features/problems/components/ProblemList.tsx` | Paginated list |
| `features/home/components/Hero.tsx` | Landing hero section |
| `features/home/components/PracticeModeCard.tsx` | Practice mode CTA |
| `features/home/components/CompetitionModeCard.tsx` | Competition mode CTA |
| `features/auth/components/LoginCard.tsx` | Login card |
| `components/ui/Button.tsx` | Generic button (+ twMerge) |
| `components/ui/Badge.tsx` | Difficulty badge (decoupled from model) |
| `components/ui/Container.tsx` | Card wrapper with light/dark variants |
| `components/ui/StatItem.tsx` | Value + label stat display |
| `components/ui/Pagination.tsx` | Page nav with ellipsis |
| `components/layout/TypeWriter.tsx` | Typewriter animation |
| `components/layout/Logo.tsx` | Brand logo with link |
| `components/layout/Navbar.tsx` | Top navigation bar |
| `components/layout/Footer.tsx` | Footer |

### Files to modify

| File | Change |
|------|--------|
| `app/layout.tsx` | Update Navbar/Footer imports |
| `app/page.tsx` | Replace inline markup with feature components |
| `app/login/page.tsx` | Replace Container usage with LoginCard |
| `app/problems/page.tsx` | Update import paths (rename ProblemSelection → ProblemFilter) |

### Files to delete

`components/Button/`, `components/Badge/`, `components/Container/`, `components/Hero/`, `components/Navbar/`, `components/Footer/`, `components/Logo/`, `components/TypeWriter/`, `components/ProblemList/`, `components/ProblemSelection/`, `components/UserRank/`, `model/`

---

## Task 1: Problem domain types and data

**Files:**
- Create: `code-sprint-fe/features/problems/types.ts`
- Create: `code-sprint-fe/features/problems/data.ts`

- [ ] **Step 1: Create types.ts**

```ts
// code-sprint-fe/features/problems/types.ts
export type Difficulty = "easy" | "medium" | "hard";
export type FilterDifficulty = "all" | Difficulty;

export type Problem = {
  id: string;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  solvedCount: number;
};
```

- [ ] **Step 2: Create data.ts**

```ts
// code-sprint-fe/features/problems/data.ts
import type { Problem } from "./types";

export const problems: Problem[] = [
  { id: "1", title: "Two Sum", difficulty: "easy", tags: ["Mathematical"], solvedCount: 12400 },
  { id: "2", title: "Valid Parentheses", difficulty: "easy", tags: ["Strings"], solvedCount: 9800 },
  { id: "3", title: "Longest Substring", difficulty: "medium", tags: ["Strings"], solvedCount: 7200 },
  { id: "4", title: "3Sum", difficulty: "medium", tags: ["Mathematical"], solvedCount: 5100 },
  {
    id: "5",
    title: "Median of Two Sorted Arrays",
    difficulty: "hard",
    tags: ["Logical"],
    solvedCount: 2300,
  },
  {
    id: "6",
    title: "Trapping Rain Water",
    difficulty: "hard",
    tags: ["Geometrics"],
    solvedCount: 1900,
  },
];
```

- [ ] **Step 3: Verify TypeScript compiles**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors in the two new files.

---

## Task 2: Shared UI primitives — Button, Badge, Container

**Files:**
- Create: `code-sprint-fe/components/ui/Button.tsx`
- Create: `code-sprint-fe/components/ui/Badge.tsx`
- Create: `code-sprint-fe/components/ui/Container.tsx`

> **Note on Container:** The original Container always renders the icon (defaulting to `TrophyIcon`) and an empty `<h2>` when `title` is not passed. This causes layout issues on the login page. Fix here: conditionally render icon and title.

- [ ] **Step 1: Create Button.tsx**

Button gains `twMerge` so `className` overrides actually win over base classes.

```tsx
// code-sprint-fe/components/ui/Button.tsx
import { ButtonHTMLAttributes } from "react";
import { twMerge } from "tailwind-merge";

type Variant = "primary" | "outline" | "ghost";
type Size = "sm" | "md" | "lg";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
}

const variants: Record<Variant, string> = {
  primary: "bg-primary-600 text-white hover:bg-primary-700",
  outline: "border border-border bg-surface text-text-primary hover:bg-neutral-50",
  ghost: "bg-transparent text-text-primary hover:bg-neutral-100",
};

const sizes: Record<Size, string> = {
  sm: "px-3 py-1.5 text-sm",
  md: "px-4 py-2 text-base",
  lg: "px-6 py-3 text-lg",
};

export function Button({
  variant = "primary",
  size = "md",
  className,
  children,
  ...props
}: ButtonProps) {
  return (
    <button
      className={twMerge(
        "cursor-pointer rounded-full font-semibold transition-colors",
        variants[variant],
        sizes[size],
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
}
```

- [ ] **Step 2: Create Badge.tsx**

Remove `import { Problem }` — Badge defines its difficulty type inline, decoupled from the problems feature.

```tsx
// code-sprint-fe/components/ui/Badge.tsx
type Difficulty = "easy" | "medium" | "hard";

const difficultyStyles: Record<Difficulty, string> = {
  easy: "bg-green-100 text-green-700",
  medium: "bg-yellow-100 text-yellow-700",
  hard: "bg-red-100 text-red-700",
};

const difficultyLabel: Record<Difficulty, string> = {
  easy: "Easy",
  medium: "Medium",
  hard: "Hard",
};

interface BadgeProps {
  difficulty: Difficulty;
}

export function Badge({ difficulty }: BadgeProps) {
  return (
    <span className={`${difficultyStyles[difficulty]} rounded-full px-3 py-1 text-sm font-semibold`}>
      {difficultyLabel[difficulty]}
    </span>
  );
}
```

- [ ] **Step 3: Create Container.tsx**

```tsx
// code-sprint-fe/components/ui/Container.tsx
import { ReactNode } from "react";

type Variant = "light" | "dark";

type ContainerProps = {
  children: ReactNode;
  icon?: ReactNode;
  title?: string;
  variant?: Variant;
};

const variantStyles = {
  light: {
    container: "bg-white",
    iconWrapper: "bg-primary-100 text-primary-500",
    title: "text-gray-900",
  },
  dark: {
    container: "bg-surface-dark",
    iconWrapper: "bg-surface-dark-elevated text-primary-500",
    title: "text-white",
  },
};

export function Container({ children, icon, title, variant = "light" }: ContainerProps) {
  const styles = variantStyles[variant];
  return (
    <div
      className={`${styles.container} mt-10 flex min-h-50 w-full max-w-md flex-col gap-0.5 rounded-2xl p-5`}
    >
      {icon && (
        <div className={`${styles.iconWrapper} h-fit w-fit rounded-lg p-3`}>{icon}</div>
      )}
      {title && (
        <h2 className={`${styles.title} mb-4 text-3xl leading-tight font-black`}>{title}</h2>
      )}
      {children}
    </div>
  );
}
```

- [ ] **Step 4: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors in new files.

---

## Task 3: Shared utilities — StatItem, Pagination

**Files:**
- Create: `code-sprint-fe/components/ui/StatItem.tsx`
- Create: `code-sprint-fe/components/ui/Pagination.tsx`

> **Note:** `StatItem` is promoted from a private function inside `UserRank`. The dark competition stat tiles in `CompetitionModeCard` have different layout (label above, `text-4xl` value, dark bg wrapper) so they stay as inline JSX — not a StatItem concern.

- [ ] **Step 1: Create StatItem.tsx**

```tsx
// code-sprint-fe/components/ui/StatItem.tsx
interface StatItemProps {
  value: string;
  label: string;
}

export function StatItem({ value, label }: StatItemProps) {
  return (
    <div className="flex flex-col">
      <p className="leading-tight font-black">{value}</p>
      <small className="text-sm text-neutral-600">{label}</small>
    </div>
  );
}
```

- [ ] **Step 2: Create Pagination.tsx**

`getPageRange` extracted from `ProblemList`. Accepts controlled `page` + `onPageChange` callback.

```tsx
// code-sprint-fe/components/ui/Pagination.tsx
"use client";

type PageItem = number | "...";

function getPageRange(current: number, total: number): PageItem[] {
  if (total <= 5) return Array.from({ length: total }, (_, i) => i + 1);
  if (current <= 3) return [1, 2, 3, "...", total];
  if (current >= total - 2) return [1, "...", total - 2, total - 1, total];
  return [1, "...", current - 1, current, current + 1, "...", total];
}

interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  return (
    <div className="mt-4 flex items-center justify-center gap-2">
      <button
        onClick={() => onPageChange(Math.max(1, page - 1))}
        disabled={page === 1}
        className="flex h-9 w-9 items-center justify-center rounded-full bg-neutral-100 disabled:opacity-40"
      >
        ‹
      </button>

      {getPageRange(page, totalPages).map((p, i) =>
        p === "..." ? (
          <span key={`ellipsis-${i}`} className="text-text-secondary px-1">
            ...
          </span>
        ) : (
          <button
            key={p}
            onClick={() => onPageChange(p)}
            className={`h-9 w-9 rounded-full font-semibold ${
              p === page ? "bg-black text-white" : "bg-neutral-100 text-neutral-700"
            }`}
          >
            {p}
          </button>
        )
      )}

      <button
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
        className="flex h-9 w-9 items-center justify-center rounded-full bg-neutral-100 disabled:opacity-40"
      >
        ›
      </button>
    </div>
  );
}
```

- [ ] **Step 3: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors.

---

## Task 4: Layout components

**Files:**
- Create: `code-sprint-fe/components/layout/TypeWriter.tsx`
- Create: `code-sprint-fe/components/layout/Logo.tsx`
- Create: `code-sprint-fe/components/layout/Navbar.tsx`
- Create: `code-sprint-fe/components/layout/Footer.tsx`

- [ ] **Step 1: Create TypeWriter.tsx**

```tsx
// code-sprint-fe/components/layout/TypeWriter.tsx
"use client";

import { useEffect, useState } from "react";

interface TypeWriterProps {
  text: string;
  speed?: number;
}

export function TypeWriter({ text, speed = 100 }: TypeWriterProps) {
  const [displayed, setDisplayed] = useState("");

  useEffect(() => {
    if (displayed.length >= text.length) return;
    const timeout = setTimeout(() => {
      setDisplayed(text.slice(0, displayed.length + 1));
    }, speed);
    return () => clearTimeout(timeout);
  }, [displayed, text, speed]);

  return (
    <span>
      {displayed}
      <span className="ml-0.5 inline-block h-[1.1em] w-2.5 animate-[blink_1s_step-end_infinite] bg-current align-middle" />
    </span>
  );
}
```

- [ ] **Step 2: Create Logo.tsx**

```tsx
// code-sprint-fe/components/layout/Logo.tsx
import Link from "next/link";
import { TypeWriter } from "@/components/layout/TypeWriter";

export function Logo() {
  return (
    <Link href="/">
      <div className="flex flex-row items-center justify-center gap-2">
        <div className="flex items-center justify-center rounded-xl bg-neutral-900 p-2">
          <span className="text-sm font-extrabold text-white">{"</>"}</span>
        </div>
        <h2 className="font-extrabold">
          <TypeWriter text="Code Sprint" speed={120} />
        </h2>
      </div>
    </Link>
  );
}
```

- [ ] **Step 3: Create Navbar.tsx**

```tsx
// code-sprint-fe/components/layout/Navbar.tsx
import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { Logo } from "@/components/layout/Logo";

export function Navbar() {
  return (
    <nav className="flex w-full justify-between py-2 pr-1 pl-1 md:px-6">
      <Logo />
      <Link href="/login">
        <Button variant="outline">Log In</Button>
      </Link>
    </nav>
  );
}
```

- [ ] **Step 4: Create Footer.tsx**

```tsx
// code-sprint-fe/components/layout/Footer.tsx
export function Footer() {
  const getCurrentYear = new Date().getFullYear();
  return (
    <footer className="mt-2 text-center text-sm">
      {`© ${getCurrentYear} Code Sprint Platform. All rights reserved.`}
    </footer>
  );
}
```

- [ ] **Step 5: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors.

---

## Task 5: Problems feature components

**Files:**
- Create: `code-sprint-fe/features/problems/components/ProblemCard.tsx`
- Create: `code-sprint-fe/features/problems/components/ProblemFilter.tsx`
- Create: `code-sprint-fe/features/problems/components/UserRank.tsx`
- Create: `code-sprint-fe/features/problems/components/ProblemList.tsx`

- [ ] **Step 1: Create ProblemCard.tsx**

Start button uses `Button` with `className` overrides (twMerge handles conflict resolution).

```tsx
// code-sprint-fe/features/problems/components/ProblemCard.tsx
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import type { Problem } from "../types";

interface ProblemCardProps {
  problem: Problem;
}

export function ProblemCard({ problem }: ProblemCardProps) {
  return (
    <div className="border-border rounded-2xl border bg-white p-4">
      <h2 className="text-xl leading-tight font-black">{problem.title}</h2>
      <small className="text-text-secondary">
        {problem.tags.join(", ")} | {problem.solvedCount.toLocaleString("en-US")} solved
      </small>
      <div className="mt-3 flex flex-row items-center justify-between">
        <div className="flex flex-row items-center gap-3">
          <Badge difficulty={problem.difficulty} />
        </div>
        <Button className="rounded-xl bg-black px-5 py-2 hover:bg-neutral-800">Start</Button>
      </div>
    </div>
  );
}
```

- [ ] **Step 2: Create ProblemFilter.tsx**

`<p>` replaced with `<button>`. Component renamed from `ProblemSelection` to `ProblemFilter`. `FilterDifficulty` imported from feature types.

```tsx
// code-sprint-fe/features/problems/components/ProblemFilter.tsx
"use client";

import { useState } from "react";
import type { FilterDifficulty } from "../types";

const filters: { label: string; value: FilterDifficulty }[] = [
  { label: "All problems", value: "all" },
  { label: "Easy", value: "easy" },
  { label: "Medium", value: "medium" },
  { label: "Hard", value: "hard" },
];

export function ProblemFilter() {
  const [selection, setSelection] = useState<FilterDifficulty>("all");

  return (
    <div className="mx-auto flex w-fit flex-row">
      {filters.map(({ label, value }) => (
        <button
          key={value}
          onClick={() => setSelection(value)}
          className={`m-2 cursor-pointer rounded-full p-2 transition-colors ${
            selection === value ? "bg-black text-white" : "bg-neutral-100 text-neutral-700"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
```

- [ ] **Step 3: Create UserRank.tsx**

Private `StatItem` fn removed; uses `StatItem` from `components/ui/`.

```tsx
// code-sprint-fe/features/problems/components/UserRank.tsx
import { StatItem } from "@/components/ui/StatItem";

export function UserRank() {
  return (
    <div className="rounded-2lg m-2 flex flex-col gap-2 rounded-lg bg-white p-2">
      <h2 className="text-2xl font-black">Your Progress</h2>
      <div className="flex flex-row justify-between">
        <StatItem value="#3,321" label="Global rank" />
        <StatItem value="42/500" label="Solved" />
        <StatItem value="7 Days" label="Streak" />
      </div>
      <div className="flex flex-col items-center">
        <h2 className="text-xl leading-tight font-black">Points</h2>
        <p className="text-sm text-neutral-600">38.4k</p>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Create ProblemList.tsx**

`getPageRange` and inline pagination removed; uses `Pagination` component. Inner card markup extracted to `ProblemCard`.

```tsx
// code-sprint-fe/features/problems/components/ProblemList.tsx
"use client";

import { useState } from "react";
import { problems } from "../data";
import { ProblemCard } from "./ProblemCard";
import { Pagination } from "@/components/ui/Pagination";

const PAGE_SIZE = 2;

export function ProblemList() {
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil(problems.length / PAGE_SIZE);
  const paginated = problems.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <div className="flex flex-col gap-2 p-2">
      {paginated.map((problem) => (
        <ProblemCard key={problem.id} problem={problem} />
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
}
```

- [ ] **Step 5: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors.

---

## Task 6: Home feature components

**Files:**
- Create: `code-sprint-fe/features/home/components/Hero.tsx`
- Create: `code-sprint-fe/features/home/components/PracticeModeCard.tsx`
- Create: `code-sprint-fe/features/home/components/CompetitionModeCard.tsx`

- [ ] **Step 1: Create Hero.tsx**

```tsx
// code-sprint-fe/features/home/components/Hero.tsx
export function Hero() {
  return (
    <section className="flex flex-col items-center gap-4 px-4 py-10 text-center">
      <h1 className="max-w-2xl text-3xl leading-tight font-black md:text-5xl">
        Master programming through practice and competition.
      </h1>
      <p className="text-text-secondary max-w-sm md:max-w-md">
        Join a community of developers solving problems across mathematics, logic, geometry, and
        strings. Choose your path below.
      </p>
    </section>
  );
}
```

- [ ] **Step 2: Create PracticeModeCard.tsx**

```tsx
// code-sprint-fe/features/home/components/PracticeModeCard.tsx
import { DumbbellIcon } from "lucide-react";
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";

export function PracticeModeCard() {
  return (
    <Container variant="light" title="Pratice Mode" icon={<DumbbellIcon />}>
      <p className="text-text-secondary">
        Hone your skills at your own pace. Browse thousands of problems categorized by difficulty
        and topic. Perfect for learning and interview prep.
      </p>
      <div className="m-1 grid grid-cols-2 gap-2 p-1">
        {["Mathematical", "Logical", "Geometrics", "Strings"].map((tag) => (
          <span
            key={tag}
            className="text-text-secondary rounded-xl bg-neutral-100 px-4 py-2 text-sm"
          >
            {tag}
          </span>
        ))}
      </div>
      <Button>Explore problems</Button>
    </Container>
  );
}
```

- [ ] **Step 3: Create CompetitionModeCard.tsx**

Competition stat tiles (label-above, `text-4xl`, dark bg) differ structurally from `StatItem` so they stay inline.

```tsx
// code-sprint-fe/features/home/components/CompetitionModeCard.tsx
import { TrophyIcon } from "lucide-react";
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";

export function CompetitionModeCard() {
  return (
    <Container variant="dark" title="Competition Mode" icon={<TrophyIcon />}>
      <p className="text-neutral-400">
        Test your limits against others globally. Participate in real-time contests, climb the
        leaderboards, and earn rating points.
      </p>

      {/* TODO: Load from backend */}
      <div className="mb-2 grid w-full grid-cols-2 gap-3">
        <div className="bg-surface-dark-elevated rounded-xl p-4 text-white">
          <small className="text-sm text-neutral-400">Active Contests</small>
          <h2 className="text-4xl leading-tight font-black">12</h2>
        </div>
        <div className="bg-surface-dark-elevated rounded-xl p-4 text-white">
          <small className="text-sm text-neutral-400">Global Rankers</small>
          <h2 className="text-4xl leading-tight font-black">150k+</h2>
        </div>
      </div>

      <Button className="w-full">View Competitions</Button>
    </Container>
  );
}
```

- [ ] **Step 4: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors.

---

## Task 7: Auth feature component

**Files:**
- Create: `code-sprint-fe/features/auth/components/LoginCard.tsx`

- [ ] **Step 1: Create LoginCard.tsx**

```tsx
// code-sprint-fe/features/auth/components/LoginCard.tsx
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";
import { FcGoogle } from "react-icons/fc";
import { FaGithub } from "react-icons/fa";

export function LoginCard() {
  return (
    <Container>
      <h2 className="max-w-2xl text-xl leading-tight font-black md:text-2xl">Welcome back</h2>
      <span className="text-text-secondary">Enter your details to access your account.</span>
      <div className="flex flex-col gap-1">
        <Button variant="outline" className="flex items-center justify-center gap-1.5">
          <FcGoogle size={20} />
          Continue with Google
        </Button>
        <Button variant="outline" className="flex items-center justify-center gap-1.5">
          <FaGithub size={20} />
          Continue with GitHub
        </Button>
      </div>
    </Container>
  );
}
```

- [ ] **Step 2: Verify TypeScript**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: no errors.

---

## Task 8: Update app pages

**Files:**
- Modify: `code-sprint-fe/app/layout.tsx`
- Modify: `code-sprint-fe/app/page.tsx`
- Modify: `code-sprint-fe/app/login/page.tsx`
- Modify: `code-sprint-fe/app/problems/page.tsx`

- [ ] **Step 1: Update app/layout.tsx**

```tsx
// code-sprint-fe/app/layout.tsx
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Problem Solver",
  description: "Competitive programming platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${inter.variable} h-full antialiased`}>
      <body className="bg-background flex min-h-full flex-col">
        <Navbar />
        <main className="flex-1">{children}</main>
        <Footer />
      </body>
    </html>
  );
}
```

- [ ] **Step 2: Update app/page.tsx**

```tsx
// code-sprint-fe/app/page.tsx
import { Hero } from "@/features/home/components/Hero";
import { PracticeModeCard } from "@/features/home/components/PracticeModeCard";
import { CompetitionModeCard } from "@/features/home/components/CompetitionModeCard";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center">
      <Hero />
      <div className="flex flex-col p-2.5 md:flex-row md:gap-5">
        <PracticeModeCard />
        <CompetitionModeCard />
      </div>
    </div>
  );
}
```

- [ ] **Step 3: Update app/login/page.tsx**

```tsx
// code-sprint-fe/app/login/page.tsx
import { LoginCard } from "@/features/auth/components/LoginCard";

export default function Login() {
  return (
    <div className="flex flex-1 items-center justify-center px-4">
      <LoginCard />
    </div>
  );
}
```

- [ ] **Step 4: Update app/problems/page.tsx**

```tsx
// code-sprint-fe/app/problems/page.tsx
import { ProblemFilter } from "@/features/problems/components/ProblemFilter";
import { UserRank } from "@/features/problems/components/UserRank";
import { ProblemList } from "@/features/problems/components/ProblemList";

export default function Problems() {
  return (
    <div className="flex flex-col">
      <ProblemFilter />
      <UserRank />
      <ProblemList />
    </div>
  );
}
```

- [ ] **Step 5: Full TypeScript check**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: zero errors. All old imports resolved to new paths.

- [ ] **Step 6: Run dev server and verify all 3 routes**

```
cd code-sprint-fe && npm run dev
```

Visit:
- `http://localhost:3000` — home page: Hero, two mode cards visible
- `http://localhost:3000/login` — login card with Google + GitHub buttons, no stray icon/title
- `http://localhost:3000/problems` — filter pills, user rank panel, paginated problem list

---

## Task 9: Delete old files and commit

> **Warning:** This deletes all old component directories and the `model/` folder. Verify Task 8 Step 5 (tsc) and Step 6 (dev server) both pass before proceeding.

- [ ] **Step 1: Delete old components and model/**

```
cd code-sprint-fe
```

Delete these directories:
- `components/Button/`
- `components/Badge/`
- `components/Container/`
- `components/Hero/`
- `components/Navbar/`
- `components/Footer/`
- `components/Logo/`
- `components/TypeWriter/`
- `components/ProblemList/`
- `components/ProblemSelection/`
- `components/UserRank/`
- `model/`

PowerShell command:
```powershell
Remove-Item -Recurse -Force `
  components/Button, `
  components/Badge, `
  components/Container, `
  components/Hero, `
  components/Navbar, `
  components/Footer, `
  components/Logo, `
  components/TypeWriter, `
  components/ProblemList, `
  components/ProblemSelection, `
  components/UserRank, `
  model
```

- [ ] **Step 2: Final TypeScript check after deletions**

```
cd code-sprint-fe && npx tsc --noEmit
```
Expected: zero errors. No remaining imports from deleted paths.

- [ ] **Step 3: Commit**

```bash
git add code-sprint-fe/features/ code-sprint-fe/components/ui/ code-sprint-fe/components/layout/ code-sprint-fe/app/ docs/
git add -u code-sprint-fe/components/ code-sprint-fe/model/
git commit -m "refactor: restructure to vertical slice architecture

- features/{home,auth,problems} own their components, types, and data
- components/ui: Button (+ twMerge), Badge, Container, StatItem, Pagination
- components/layout: Navbar, Footer, Logo, TypeWriter
- extract ProblemCard, ProblemFilter (was ProblemSelection), Pagination
- fix Container conditional rendering of icon/title
- replace <p> buttons in ProblemFilter with <button>
- decouple Badge from model/ — defines Difficulty inline
- delete model/ and flat components/ dirs"
```
