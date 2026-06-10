import { getSubmission } from "@/services/submission";
import { useQuery } from "@tanstack/react-query";

// Single fetch, no polling (see worker stub: verdict resolves ~1.5s after submit,
// so an early load may still read pending/running).
export function useSubmission(id: string) {
  return useQuery({
    queryKey: ["submission", id],
    queryFn: () => getSubmission(id),
    enabled: !!id,
  });
}
