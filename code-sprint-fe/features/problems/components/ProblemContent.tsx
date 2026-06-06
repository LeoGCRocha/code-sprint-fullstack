"use client";

import { useState } from "react";
import { ProblemFilter } from "./ProblemFilter";
import { ProblemList } from "./ProblemList";
import { useProblems } from "../hooks/useProblems";
import type { FilterDifficulty } from "../types";

const PAGE_SIZE = 4;

// TODO: Add status by relation between client and problem

export function ProblemContent() {
  const [page, setPage] = useState(1);
  const [difficulty, setDifficulty] = useState<FilterDifficulty | undefined>(undefined);

  const { data, isLoading } = useProblems({
    difficulty: difficulty === "all" ? undefined : difficulty,
    page,
    pageSize: PAGE_SIZE,
  });

  const totalPages = Math.ceil((data?.total ?? 0) / PAGE_SIZE);

  return (
    <>
      <ProblemFilter onClick={(v) => { setDifficulty(v); setPage(1); }} selection={difficulty} />
      <ProblemList
        items={data?.items ?? []}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        isLoading={isLoading}
      />
    </>
  );
}
