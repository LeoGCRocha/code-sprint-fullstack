import type { Problem } from "./types";

export const problems: Problem[] = [
  { id: "1", title: "Two Sum", difficulty: "easy", tags: ["Mathematical"], solvedCount: 12400 },
  { id: "2", title: "Valid Parentheses", difficulty: "easy", tags: ["Strings"], solvedCount: 9800 },
  { id: "3", title: "Longest Substring", difficulty: "medium", tags: ["Strings"], solvedCount: 7200 },
  { id: "4", title: "3Sum", difficulty: "medium", tags: ["Mathematical"], solvedCount: 5100 },
  {
    id: "5",
    title: "Median of Two Sorted Arrays",
    difficulty: "hard",
    tags: ["Logical"],
    solvedCount: 2300,
  },
  {
    id: "6",
    title: "Trapping Rain Water",
    difficulty: "hard",
    tags: ["Geometrics"],
    solvedCount: 1900,
  },
];
