import { createSubmission } from "@/services/submission";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";

interface UseSubmitOptions {
  onSuccess?: () => void;
}

export function useSubmit(options?: UseSubmitOptions) {
  const router = useRouter();

  return useMutation({
    mutationFn: createSubmission,
    onSuccess: ({ id }) => {
      options?.onSuccess?.();          // caller hook (e.g. clearDraft)
      router.push(`/submissions/${id}`);
    },
  });
}
