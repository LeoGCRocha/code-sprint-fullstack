import { Badge, type BadgeVariant } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import type { Difficulty, Problem } from "../types";
import Link from "next/link";

const difficultyBadge: Record<Difficulty, { variant: BadgeVariant; label: string }> = {
  easy: { variant: "green", label: "Easy" },
  medium: { variant: "yellow", label: "Medium" },
  hard: { variant: "red", label: "Hard" },
};

export function ProblemRow({ problem, index }: { problem: Problem; index: number }) {
  const buttonLabel = { start: "Start", continue: "Continue", review: "Review" }[problem.status];
  const buttonVariant = problem.status === "review" ? "outline" : "primary";

  return (
    <div className="grid grid-cols-[40px_1fr_auto_auto_auto] items-center gap-4 rounded-2xl bg-white p-4">
      <span className="text-center font-mono text-sm text-neutral-400">
        {String(index).padStart(2, "0")}
      </span>
      <div>
        <Link href={`/problems/${problem.slug}`} className="font-semibold hover:underline">
          {problem.title}
        </Link>
        <small className="text-neutral-400">
          {problem.tags.join(", ")} ·{problem.estimatedTime} ·{" "}
          {problem.solvedCount.toLocaleString("en-US")} solved
        </small>
      </div>
      <Badge variant={difficultyBadge[problem.difficulty].variant}>
        {difficultyBadge[problem.difficulty].label}
      </Badge>
      <span className="font-black">{problem.points} pts</span>
      {/* TODO: Should pass the localContext if the user has already implemented an solution */}
      <Link href={`/problems/${problem.slug}/solution`}>
        <Button variant={buttonVariant} size="sm">
          {buttonLabel}
        </Button>
      </Link>
    </div>
  );
}
