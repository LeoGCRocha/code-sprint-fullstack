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
    <div className="mb-4 flex flex-row flex-wrap gap-1">
      {filters.map(({ label, value }) => (
        <button
          key={value}
          onClick={() => setSelection(value)}
          className={`cursor-pointer rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
            selection === value ? "bg-primary-600 text-white" : "bg-neutral-100 text-neutral-700 hover:bg-neutral-200"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
