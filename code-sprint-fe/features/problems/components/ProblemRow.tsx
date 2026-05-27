import { Badge, type BadgeVariant } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import type { Difficulty, Problem } from "../types";

const difficultyBadge: Record<Difficulty, { variant: BadgeVariant; label: string }> = {
  easy: { variant: "green", label: "Easy" },
  medium: { variant: "yellow", label: "Medium" },
  hard: { variant: "red", label: "Hard" },
};

export function ProblemRow({ problem, index }: { problem: Problem; index: number }) {
  const buttonLabel = { start: "Start", continue: "Continue", review: "Review" }[problem.status];
  const buttonVariant = problem.status === "review" ? "outline" : "primary";

  return (
    <div className="grid grid-cols-[40px_1fr_auto_auto_auto] items-center gap-4 rounded-xl bg-white p-4">
      <span className="text-center font-mono text-sm text-neutral-400">
        {String(index).padStart(2, "0")}
      </span>
      <div>
        <p className="font-semibold">{problem.title}</p>
        <small className="text-neutral-400">
          {problem.tags.join(", ")} ·{problem.estimatedTime} ·{" "}
          {problem.solvedCount.toLocaleString("en-US")} solved
        </small>
      </div>
      <Badge variant={difficultyBadge[problem.difficulty].variant}>
        {difficultyBadge[problem.difficulty].label}
      </Badge>
      <span className="font-black">{problem.points} pts</span>
      <Button variant={buttonVariant} size="sm">
        {buttonLabel}
      </Button>
    </div>
  );
}
