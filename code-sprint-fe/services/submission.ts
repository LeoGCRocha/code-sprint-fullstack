export interface CreateSubmissionInput {
  problemId: string;
  language: string;
  sourceCode: string;
}

export interface CreateSubmissionResult {
  id: string;
  status: string;
}

export type SubmissionStatus = "pending" | "running" | "completed" | "failed";

export interface TestCaseResult {
  ordinal: number;
  status: string;
  runtimeMs: number;
  memoryKb: number;
  isHidden: boolean;
  actualOutput: string | null;
}

export interface SubmissionEvaluation {
  verdict: string;
  pointsAwarded: number;
  runtimeMs: number;
  memoryKb: number;
  evaluatedAt: string;
  results: TestCaseResult[];
}

export interface Submission {
  id: string;
  problemId: string;
  language: string;
  status: SubmissionStatus;
  submittedAt: string;
  evaluation: SubmissionEvaluation | null;
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
export async function getSubmission(id: string): Promise<Submission> {
  const res = await fetch(`/api/submissions/${id}`);
  if (!res.ok) throw new Error(`getSubmission failed: HTTP ${res.status}`);
  return res.json();
}
