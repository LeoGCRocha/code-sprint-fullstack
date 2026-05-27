"use client";

import { Fragment, useState } from "react";
import { problems } from "../data";
import { ProblemCard } from "./ProblemCard";
import { ProblemRow } from "./ProblemRow";
import { Pagination } from "@/components/ui/Pagination";

const PAGE_SIZE = 4;

export function ProblemList() {
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil(problems.length / PAGE_SIZE);
  const paginated = problems.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <div className="flex flex-col gap-2">
      {paginated.map((problem, i) => (
        <Fragment key={problem.id}>
          <div className="md:hidden">
            <ProblemCard problem={problem} />
          </div>
          <div className="hidden md:block">
            <ProblemRow problem={problem} index={(page - 1) * PAGE_SIZE + i + 1} />
          </div>
        </Fragment>
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
}
