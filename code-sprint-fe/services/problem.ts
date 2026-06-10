export type Difficulty = "easy" | "medium" | "hard";

export type Problem = {
  id: string;
  slug: string;
  title: string;
  difficulty: Difficulty;
  points: number;
  estimatedTime: string;
  solvedCount: number;
  tags: string[];
  status?: "start" | "continue" | "review";
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
  pageSize?: number;
}

export async function fetchProblemBySlug(slug: string): Promise<Problem | null> {
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/problems/${encodeURIComponent(slug)}`, {
    cache: "no-store",
  });

  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`HTTP ${res.status}`);

  return res.json();
}

export async function fetchProblems(filter: ProblemsFilter): Promise<ProblemsPage> {
  const params = new URLSearchParams();
  if (filter.difficulty) params.set("difficulty", filter.difficulty);
  if (filter.page) params.set("page", String(filter.page));
  if (filter.pageSize) params.set("pageSize", String(filter.pageSize));

  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/problems?${params}`);

  if (!res.ok) throw new Error(`HTTTP ${res.status}`);

  return res.json();
}
