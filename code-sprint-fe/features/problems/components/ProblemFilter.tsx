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
