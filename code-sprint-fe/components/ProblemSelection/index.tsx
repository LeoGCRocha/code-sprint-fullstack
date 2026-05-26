"use client";

import { useState } from "react";

type Difficulty = "all" | "easy" | "medium" | "hard";

const filters: { label: string; value: Difficulty }[] = [
  { label: "All problems", value: "all" },
  { label: "Easy", value: "easy" },
  { label: "Medium", value: "medium" },
  { label: "Hard", value: "hard" },
];

export function ProblemSelection() {
  const [selection, setSelection] = useState<Difficulty>("all");

  // TODO: How to fast update this.

  return (
    <div className="mx-auto flex w-fit flex-row">
      {filters.map(({ label, value }) => (
        <p
          key={value}
          onClick={() => setSelection(value)}
          className={`m-2 cursor-pointer rounded-full p-2 transition-colors ${
            selection === value ? "bg-black text-white" : "bg-neutral-100 text-neutral-700"
          }`}
        >
          {label}
        </p>
      ))}
    </div>
  );
}
