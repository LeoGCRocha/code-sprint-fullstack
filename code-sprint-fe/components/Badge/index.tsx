import type { Problem } from "@/model/Problem";

type Difficulty = Problem["difficulty"];

const difficultyStyles: Record<Difficulty, string> = {
  easy: "bg-green-100 text-green-700",
  medium: "bg-yellow-100 text-yellow-700",
  hard: "bg-red-100 text-red-700",
};

const difficultyLabel: Record<Difficulty, string> = {
  easy: "Easy",
  medium: "Medium",
  hard: "Hard",
};

interface BadgeProps {
  difficulty: Difficulty;
}

export function Badge({ difficulty }: BadgeProps) {
  return (
    <span
      className={`${difficultyStyles[difficulty]} rounded-full px-3 py-1 text-sm font-semibold`}
    >
      {difficultyLabel[difficulty]}
    </span>
  );
}
