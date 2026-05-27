export type Difficulty = "easy" | "medium" | "hard";
export type FilterDifficulty = "all" | Difficulty;

export type Problem = {
  id: string;
  title: string;
  difficulty: Difficulty;
  tags: string[];
  solvedCount: number;
};
