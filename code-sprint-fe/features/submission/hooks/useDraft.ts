import { useEffect, useState } from "react";

export const key = (problemId: string, userId?: string) => `draft:${userId ?? "anon"}:${problemId}`;

export function useDraft(problemId: string, userId?: string, initial = "") {
  const [code, setCode] = useState(() =>
    typeof window === "undefined"
      ? initial
      : (localStorage.getItem(key(problemId, userId)) ?? initial)
  );

  useEffect(() => {
    const t = setTimeout(() => localStorage.setItem(key(problemId, userId), code), 500);
    return () => clearTimeout(t);
  }, [problemId, userId, code]);

  const clearDraft = () => localStorage.removeItem(key(problemId, userId));
  const hasDraft = () =>
    typeof window !== "undefined" && localStorage.getItem(key(problemId, userId)) !== null;

  return { code, setCode, clearDraft, hasDraft };
}
