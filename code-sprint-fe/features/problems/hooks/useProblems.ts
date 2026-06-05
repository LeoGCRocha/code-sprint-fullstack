import { fetchProblems, ProblemsFilter } from "@/services/problem";
import { useQuery } from "@tanstack/react-query";

export function useProblems(filter: ProblemsFilter) {
  return useQuery({
    queryKey: ["problems", filter],
    queryFn: () => fetchProblems(filter),
  });
}
