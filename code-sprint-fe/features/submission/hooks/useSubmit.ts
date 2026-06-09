import { createSubmission } from "@/services/submission";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";

export function useSubmit() {
  const router = useRouter();

  return useMutation({
    mutationFn: createSubmission,
    onSuccess: ({ id }) => router.push(`/submissions/${id}`),
  });
}
