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
