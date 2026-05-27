export type Difficulty = "easy" | "medium" | "hard";
export type FilterDifficulty = "all" | Difficulty;
export type ProblemStatus = "start" | "continue" | "review";

export type Problem = {
  id: string;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  solvedCount: number;
  estimatedTime: string;
  points: number;
  status: ProblemStatus;
};
