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
