import type { CreateSubmissionResult } from "@/services/submission";

export const mockSubmissionAck: CreateSubmissionResult = {
  id: "mock-sub-001",
  status: "Pending",
};

export const mockSubmissionResponse = {
  id: "mock-sub-001",
  problemId: "two-sum",
  language: "python3",
  status: "Completed",
  submittedAt: "2026-06-08T14:32:00.000Z",
  evaluation: {
    verdict: "Accepted",
    pointsAwarded: 50,
    runtimeMs: 12,
    memoryKb: 4200,
    evaluatedAt: "2026-06-08T14:32:01.000Z",
    results: [
      { ordinal: 1, status: "Accepted", runtimeMs: 4, memoryKb: 4100, isHidden: false, actualOutput: null },
      { ordinal: 2, status: "Accepted", runtimeMs: 5, memoryKb: 4100, isHidden: false, actualOutput: null },
      { ordinal: 3, status: "Accepted", runtimeMs: 3, memoryKb: 4000, isHidden: false, actualOutput: null },
    ],
  },
};

export const mockSubmissionWrongAnswer = {
  ...mockSubmissionResponse,
  id: "mock-sub-002",
  evaluation: {
    ...mockSubmissionResponse.evaluation,
    verdict: "WrongAnswer",
    pointsAwarded: 0,
    results: [
      { ordinal: 1, status: "Accepted", runtimeMs: 4, memoryKb: 4100, isHidden: false, actualOutput: null },
      { ordinal: 2, status: "WrongAnswer", runtimeMs: 5, memoryKb: 4100, isHidden: false, actualOutput: "[1,0]" },
      { ordinal: 3, status: "Accepted", runtimeMs: 3, memoryKb: 4000, isHidden: false, actualOutput: null },
    ],
  },
};
