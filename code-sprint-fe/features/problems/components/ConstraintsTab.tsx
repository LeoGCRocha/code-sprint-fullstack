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
