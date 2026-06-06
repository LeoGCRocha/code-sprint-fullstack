"use client";

import { Fragment } from "react";
import { ProblemCard } from "./ProblemCard";
import { ProblemRow } from "./ProblemRow";
import { Pagination } from "@/components/ui/Pagination";
import type { Problem } from "@/services/problem";

const PAGE_SIZE = 4;

type ProblemListProps = {
  items: Problem[];
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  isLoading: boolean;
};

export function ProblemList({
  items,
  page,
  totalPages,
  onPageChange,
  isLoading,
}: ProblemListProps) {
  if (isLoading) return <div className="py-8 text-center text-neutral-500">Loading...</div>;

  return (
    <div className="flex flex-col gap-2">
      {items.map((problem, i) => (
        <Fragment key={problem.id}>
          <div className="md:hidden">
            <ProblemCard problem={problem} />
          </div>
          <div className="hidden md:block">
            <ProblemRow problem={problem} index={(page - 1) * PAGE_SIZE + i + 1} />
          </div>
        </Fragment>
      ))}
      <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />
    </div>
  );
}
