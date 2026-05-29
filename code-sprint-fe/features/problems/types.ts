export type Difficulty = "easy" | "medium" | "hard";
export type FilterDifficulty = "all" | Difficulty;
export type ProblemStatus = "start" | "continue" | "review";

export type Example = {
  input: string;
  output: string;
  explanation?: string;
};

export type Problem = {
  id: string;
  slug: string;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  solvedCount: number;
  estimatedTime: string;
  points: number;
  status: ProblemStatus;
  description: string;
  notes: string[];
  inputFormat: string[];
  constraints: string[];
  examples: Example[];
};
