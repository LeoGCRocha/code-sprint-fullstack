"use client";

import { useState } from "react";
import { problems } from "../data";
import { ProblemCard } from "./ProblemCard";
import { Pagination } from "@/components/ui/Pagination";

const PAGE_SIZE = 2;

export function ProblemList() {
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil(problems.length / PAGE_SIZE);
  const paginated = problems.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <div className="flex flex-col gap-2 p-2">
      {paginated.map((problem) => (
        <ProblemCard key={problem.id} problem={problem} />
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
}
