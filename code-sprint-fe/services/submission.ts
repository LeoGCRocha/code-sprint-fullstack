export interface CreateSubmissionInput {
  problemId: string;
  language: string;
  sourceCode: string;
}

export interface CreateSubmissionResult {
  id: string;
  status: string;
}

export async function createSubmission(
  input: CreateSubmissionInput
): Promise<CreateSubmissionResult> {
  const res = await fetch("/api/submissions", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });

  if (!res.ok) throw new Error(`HTTP ${res.status}`);

  return res.json();
}

// TODO: Create the web-sockets params
export async function getSubmission(id: string) {
  const res = await fetch(`/api/submissions/${id}`);
  if (!res.ok) throw new Error(`getSubmission failed: HTTP ${res.status}`);
  return res.json();
}
