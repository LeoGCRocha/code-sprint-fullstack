import type { Problem } from "@/features/problems/types";

export interface ProblemsFilter {
  difficulty?: string;
  tag?: string;
  page?: number;
}

export interface ProblemsPage {
  items: Problem[];
  total: number;
  page: number;
  pageSize: number;
}

export async function fetchProblems(filters: ProblemsFilter = {}): Promise<ProblemsPage> {
  const params = new URLSearchParams();
  if (filters.difficulty) params.set("difficulty", filters.difficulty);
  if (filters.tag) params.set("tag", filters.tag);
  if (filters.page) params.set("page", String(filters.page));

  const res = await fetch(`${process.env.NEXT_PUBLIC_PROBLEMS_API_URL}/problems?${params}`);
  if (!res.ok) throw new Error(`Failed to fetch problems: HTTP ${res.status}`);
  return res.json();
}
