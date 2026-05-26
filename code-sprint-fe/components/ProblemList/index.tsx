"use client";

import { problems } from "@/model/Problem";
import { Badge } from "@/components/Badge";
import { useState } from "react";

// TODO: Get values from the backend / Tanstack query
// TODO: Cursor pagination

const PAGE_SIZE = 2;

function getPageRange(current: number, total: number): (number | "...")[] {
  if (total <= 5) return Array.from({ length: total }, (_, i) => i + 1);

  if (current <= 3) return [1, 2, 3, "...", total];
  if (current >= total - 2) return [1, "...", total - 2, total - 1, total];
  return [1, "...", current - 1, current, current + 1, "...", total];
}

export function ProblemList() {
  const [page, setPage] = useState(1);

  const totalPages = Math.ceil(problems.length / PAGE_SIZE);
  const paginated = problems.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <div className="flex flex-col gap-2 p-2">
      {paginated.map((problem) => (
        <div key={problem.id} className="border-border rounded-2xl border bg-white p-4">
          <h2 className="text-xl leading-tight font-black">{problem.title}</h2>
          <small className="text-text-secondary">
            {problem.tags.join(", ")} | {problem.solvedCount.toLocaleString("en-US")} solved
          </small>
          <div className="mt-3 flex flex-row items-center justify-between">
            <div className="flex flex-row items-center gap-3">
              <Badge difficulty={problem.difficulty} />
            </div>
            <button className="cursor-pointer rounded-xl bg-black px-5 py-2 font-semibold text-white">
              Start
            </button>
          </div>
        </div>
      ))}

      <div className="mt-4 flex items-center justify-center gap-2">
        {/* Prev arrow */}
        <button
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1}
          className="flex h-9 w-9 items-center justify-center rounded-full bg-neutral-100 disabled:opacity-40"
        >
          ‹
        </button>

        {/* Pages + ellipsis */}
        {getPageRange(page, totalPages).map((p, i) =>
          p === "..." ? (
            <span key={`ellipsis-${i}`} className="text-text-secondary px-1">
              ...
            </span>
          ) : (
            <button
              key={p}
              onClick={() => setPage(p)}
              className={`h-9 w-9 rounded-full font-semibold ${
                p === page ? "bg-black text-white" : "bg-neutral-100 text-neutral-700"
              }`}
            >
              {p}
            </button>
          )
        )}

        {/* Next arrow */}
        <button
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          disabled={page === totalPages}
          className="flex h-9 w-9 items-center justify-center rounded-full bg-neutral-100 disabled:opacity-40"
        >
          ›
        </button>
      </div>
    </div>
  );
}
