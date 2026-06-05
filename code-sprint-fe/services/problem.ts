export type Difficulty = "easy" | "medium" | "hard";

export type Problem = {
  id: string;
  slug: string;
  title: string;
  difficulty: Difficulty;
  points: number;
  solvedCount: number;
  tags: string[];
};

export interface ProblemsPage {
  items: Problem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ProblemsFilter {
  difficulty?: Difficulty | undefined;
  tag?: string;
  page?: number;
}

export async function fetchProblems(filter: ProblemsFilter): Promise<ProblemsPage> {
  const params = new URLSearchParams();
  if (filter.difficulty) params.set("difficulty", filter.difficulty);
  if (filter.page) params.set("page", String(filter.page));

  const res = await fetch(`/api/problems?${params}`);

  if (!res.ok) throw new Error(`HTTTP ${res.status}`);

  return res.json();
}
