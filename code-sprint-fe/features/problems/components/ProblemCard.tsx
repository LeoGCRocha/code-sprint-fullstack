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
