import { Badge, type BadgeVariant } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import type { Difficulty } from "../types";
import type { Problem } from "@/services/problem";
import Link from "next/link";

const difficultyBadge: Record<Difficulty, { variant: BadgeVariant; label: string }> = {
  easy: { variant: "green", label: "Easy" },
  medium: { variant: "yellow", label: "Medium" },
  hard: { variant: "red", label: "Hard" },
};

interface ProblemCardProps {
  problem: Problem;
}

export function ProblemCard({ problem }: ProblemCardProps) {
  const status = problem.status ?? "start";
  const buttonLabel = { start: "Start", continue: "Continue", review: "Review" }[status];
  const buttonVariant = status === "review" ? "outline" : "primary";

  return (
    <div className="border-border rounded-2xl border bg-white p-5">
      <h2 className="text-xl leading-tight font-black">
        <Link href={`/problems/${problem.slug}`}>{problem.title}</Link>
      </h2>
      <small className="text-text-secondary">
        {problem.tags.join(", ")} | {problem.solvedCount.toLocaleString("en-US")} solved
      </small>
      <div className="mt-3 flex flex-row items-center justify-between">
        <div className="flex flex-row items-center gap-3">
          <Badge variant={difficultyBadge[problem.difficulty].variant}>
            {difficultyBadge[problem.difficulty].label}
          </Badge>
        </div>
        <Link href={`/problems/${problem.slug}/solution`}>
          <Button variant={buttonVariant} className="rounded-xl px-5 py-2">
            {buttonLabel}
          </Button>
        </Link>
      </div>
    </div>
  );
}
