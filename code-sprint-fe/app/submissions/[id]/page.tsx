// Real data: status/verdict, language, submittedAt, runtime, memory, points, test cases.
// TODO(BE): these are still static placeholders — the submission API does not return them:
//   - problem title/difficulty (submission only carries a problemId GUID; no GET /problems/{id})
//   - runtime/memory percentile ("beats X% of users")
//   - competition impact (rankShift)
// "Next Problem" still needs the next unsolved problem from BE.
"use client";

import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { CheckIcon, CodeIcon, ArrowRightIcon, XIcon, TrendingUpIcon, Loader2Icon } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useSubmission } from "@/features/submission/hooks/useSubmission";

const verdictConfig = {
  accepted: {
    Icon: CheckIcon,
    label: "ACCEPTED",
    iconBg: "bg-green-100",
    iconColor: "text-green-600",
    textColor: "text-green-600",
  },
  rejected: {
    Icon: XIcon,
    label: "REJECTED",
    iconBg: "bg-red-100",
    iconColor: "text-red-600",
    textColor: "text-red-600",
  },
} as const;

function Skeleton({ className = "" }: { className?: string }) {
  return <div className={`animate-pulse rounded-md bg-neutral-200 ${className}`} />;
}

// Mirrors the result layout, but every data slot is a shimmer placeholder and the
// status card spins. Used while the submission is still pending/running (no polling,
// so it resolves on the next manual load).
function JudgingView() {
  return (
    <div className="bg-background min-h-screen px-4 py-6 md:px-6">
      <div className="mx-auto max-w-5xl">
        <div className="grid gap-4 md:grid-cols-[1fr_1.4fr]">

          {/* Left column */}
          <div className="flex flex-col gap-4 md:sticky md:top-6 md:self-start">
            {/* Status — live spinner */}
            <div className="rounded-2xl bg-white p-5">
              <div className="flex items-center gap-3">
                <div className="rounded-full bg-amber-100 p-2">
                  <Loader2Icon className="h-6 w-6 animate-spin text-amber-600" />
                </div>
                <div>
                  <h2 className="text-xl font-black text-amber-600">JUDGING…</h2>
                  <p className="text-sm text-neutral-500">Running your code against the test cases.</p>
                </div>
              </div>
              <div className="mt-4 flex gap-2">
                <Skeleton className="h-10 flex-1" />
                <Skeleton className="h-10 w-20" />
              </div>
            </div>

            {/* Competition Impact */}
            <div className="rounded-2xl bg-white p-5">
              <div className="mb-3 flex items-center gap-2">
                <TrendingUpIcon className="h-4 w-4 text-neutral-300" />
                <h4 className="font-bold text-neutral-400">Competition Impact</h4>
              </div>
              <div className="grid grid-cols-2 gap-4 rounded-xl bg-neutral-100 p-4">
                <div className="space-y-2">
                  <Skeleton className="h-3 w-16" />
                  <Skeleton className="h-7 w-20" />
                </div>
                <div className="space-y-2">
                  <Skeleton className="h-3 w-16" />
                  <Skeleton className="h-7 w-20" />
                </div>
              </div>
            </div>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-4">
            {/* Problem Info */}
            <div className="rounded-2xl bg-white p-5">
              <div className="flex gap-2">
                <Skeleton className="h-6 w-20 rounded-full" />
                <Skeleton className="h-6 w-24 rounded-full" />
              </div>
              <Skeleton className="mt-3 h-6 w-2/3" />
              <Skeleton className="mt-2 h-4 w-40" />
              <hr className="my-4 border-neutral-100" />
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-4 w-32" />
                </div>
                <div className="space-y-2">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-4 w-28" />
                </div>
              </div>
              <div className="mt-4 grid grid-cols-2 gap-3">
                <div className="space-y-2 rounded-xl bg-neutral-50 p-4">
                  <Skeleton className="h-3 w-16" />
                  <Skeleton className="h-7 w-20" />
                  <Skeleton className="h-5 w-28 rounded-full" />
                </div>
                <div className="space-y-2 rounded-xl bg-neutral-50 p-4">
                  <Skeleton className="h-3 w-16" />
                  <Skeleton className="h-7 w-20" />
                  <Skeleton className="h-5 w-28 rounded-full" />
                </div>
              </div>
            </div>

            {/* Test Case Details */}
            <div className="rounded-2xl bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <h4 className="font-bold text-neutral-400">Test Case Details</h4>
                <Skeleton className="h-5 w-20 rounded-full" />
              </div>
              <div className="space-y-2">
                {[0, 1, 2].map((i) => (
                  <div
                    key={i}
                    className="flex items-center justify-between rounded-xl bg-neutral-50 px-4 py-3"
                  >
                    <div className="space-y-2">
                      <Skeleton className="h-4 w-24" />
                      <Skeleton className="h-3 w-40" />
                    </div>
                    <Loader2Icon className="h-4 w-4 animate-spin text-neutral-300" />
                  </div>
                ))}
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}

export default function SubmissionPage() {
  const { id } = useParams<{ id: string }>();
  const { data: submission, isLoading, isError } = useSubmission(id);

  if (isLoading) return <JudgingView />;

  if (isError || !submission) {
    return (
      <div className="bg-background flex min-h-screen items-center justify-center">
        <p className="text-sm text-red-600">Could not load submission.</p>
      </div>
    );
  }

  const { status, language, submittedAt, evaluation } = submission;

  // Failed = judge infrastructure error (no evaluation produced).
  if (status === "failed") {
    return (
      <div className="bg-background flex min-h-screen items-center justify-center">
        <div className="rounded-2xl bg-white p-6 text-center">
          <div className="mx-auto mb-3 w-fit rounded-full bg-red-100 p-2">
            <XIcon className="h-6 w-6 text-red-600" />
          </div>
          <p className="text-lg font-black text-red-600">JUDGE ERROR</p>
          <p className="mt-1 text-sm text-neutral-500">The judge could not evaluate this submission.</p>
        </div>
      </div>
    );
  }

  // No polling: a freshly-redirected submission may still be pending/running.
  if (status !== "completed" || !evaluation) return <JudgingView />;

  const accepted = evaluation.verdict.toLowerCase() === "accepted";
  const { Icon, label, iconBg, iconColor, textColor } = accepted
    ? verdictConfig.accepted
    : verdictConfig.rejected;

  const results = evaluation.results;
  const passedCount = results.filter((r) => r.status.toLowerCase() === "passed").length;
  const description = accepted
    ? `All ${results.length} test cases passed within limits.`
    : `${passedCount}/${results.length} test cases passed.`;

  const runtimeSeconds = (evaluation.runtimeMs / 1000).toFixed(3);
  const memoryMb = (evaluation.memoryKb / 1024).toFixed(1);
  const submittedLabel = new Date(submittedAt).toLocaleString();

  return (
    <div className="bg-background min-h-screen px-4 py-6 md:px-6">
      <div className="mx-auto max-w-5xl">
        <div className="grid gap-4 md:grid-cols-[1fr_1.4fr]">

          {/* Left column: Status + Competition Impact */}
          <div className="flex flex-col gap-4 md:sticky md:top-6 md:self-start">
            {/* Status */}
            <div className="rounded-2xl bg-white p-5">
              <div className="flex items-center gap-3">
                <div className={`rounded-full p-2 ${iconBg}`}>
                  <Icon className={`h-6 w-6 ${iconColor}`} />
                </div>
                <div>
                  <h2 className={`text-xl font-black ${textColor}`}>{label}</h2>
                  <p className="text-sm text-neutral-500">{description}</p>
                </div>
              </div>

              <div className="mt-4 flex gap-2">
                {/* TODO(BE): link to next unsolved problem. */}
                <Button className="flex flex-1 items-center justify-center gap-2">
                  Next Problem <ArrowRightIcon className="h-4 w-4" />
                </Button>
                <Button variant="outline" className="flex items-center gap-2">
                  <CodeIcon className="h-4 w-4" /> Code
                </Button>
              </div>
            </div>

            {/* Competition Impact — TODO(BE): not returned by the submission API. */}
            <div className="rounded-2xl bg-white p-5">
              <div className="mb-3 flex items-center gap-2">
                <TrendingUpIcon className="h-4 w-4 text-neutral-600" />
                <h4 className="font-bold">Competition Impact</h4>
              </div>
              <div className="grid grid-cols-2 gap-4 rounded-xl bg-neutral-100 p-4">
                <div>
                  <p className="text-xs text-neutral-600">Rank Shift</p>
                  <p className="text-2xl font-black">
                    #42 <span className="text-base font-bold text-green-600">+12</span>
                  </p>
                </div>
                <div>
                  <p className="text-xs text-neutral-600">Points Earned</p>
                  <p className="text-2xl font-black">
                    +{evaluation.pointsAwarded}{" "}
                    <span className="text-base font-semibold text-neutral-600">pts</span>
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Right column: Problem Info + Test Cases */}
          <div className="flex flex-col gap-4">
            {/* Problem Info */}
            <div className="rounded-2xl bg-white p-5">
              {/* TODO(BE): problem title/difficulty/points not on the submission payload. */}
              <div className="flex gap-2">
                <Badge variant="yellow">Medium</Badge>
                <Badge variant="red">{evaluation.pointsAwarded} Points</Badge>
              </div>
              <h3 className="mt-3 text-lg leading-tight font-black">
                Prime Factorization Optimization
              </h3>
              <p className="text-sm text-neutral-500">Problem ID: #{submission.problemId}</p>

              <hr className="my-4 border-neutral-100" />

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-neutral-600">Submitted</p>
                  <p className="text-sm font-semibold">{submittedLabel}</p>
                </div>
                <div>
                  <p className="text-xs text-neutral-600">Language</p>
                  <p className="text-sm font-semibold">{language}</p>
                </div>
              </div>

              <div className="mt-4 grid grid-cols-2 gap-3">
                <div className="rounded-xl bg-neutral-50 p-4">
                  <p className="text-xs text-neutral-600">Runtime</p>
                  <p className="text-2xl font-black">{runtimeSeconds}s</p>
                  {/* TODO(BE): percentile not returned. */}
                  <Badge variant="green" className="mt-1">Beats 94.2% of users</Badge>
                </div>
                <div className="rounded-xl bg-neutral-50 p-4">
                  <p className="text-xs text-neutral-600">Memory</p>
                  <p className="text-2xl font-black">{memoryMb} MB</p>
                  {/* TODO(BE): percentile not returned. */}
                  <Badge variant="green" className="mt-1">Beats 88.5% of users</Badge>
                </div>
              </div>
            </div>

            {/* Test Case Details */}
            <div className="rounded-2xl bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <h4 className="font-bold">Test Case Details</h4>
                <span className="rounded-full bg-green-100 px-3 py-0.5 text-xs font-semibold text-green-700">
                  {passedCount}/{results.length} Passed
                </span>
              </div>
              <div className="space-y-2">
                {results.map((tc) => {
                  const passed = tc.status.toLowerCase() === "passed";
                  return (
                    <div
                      key={tc.ordinal}
                      className="flex items-center justify-between rounded-xl bg-neutral-50 px-4 py-3"
                    >
                      <div>
                        <p className="text-sm font-semibold">
                          Test Case {tc.ordinal}
                          {tc.isHidden && (
                            <span className="ml-2 text-xs font-normal text-neutral-400">(hidden)</span>
                          )}
                        </p>
                        <p className="text-xs text-neutral-600">
                          Time {(tc.runtimeMs / 1000).toFixed(3)}s &nbsp; Mem{" "}
                          {(tc.memoryKb / 1024).toFixed(1)} MB
                        </p>
                      </div>
                      <span
                        className={`text-xs font-semibold ${passed ? "text-green-600" : "text-red-600"}`}
                      >
                        {passed ? "✓ Pass" : "✗ Fail"}
                      </span>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}
