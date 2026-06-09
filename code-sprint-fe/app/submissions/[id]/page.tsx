// TODO(BE): All data on this page is hardcoded — replace with real API:
// - Fetch submission by ID from GET /submissions/:id
// - Response should include: status, problem metadata, language, submittedAt,
//   runtime, memory, percentile beats, competitionImpact (rankShift, pointsEarned),
//   and testCases array with per-case pass/fail, time, memory
// - "Next Problem" button should link to next unsolved problem from BE
// - "Code" button should link back to /problems/:slug/solution
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { CheckIcon, CodeIcon, ArrowRightIcon, XIcon, TrendingUpIcon } from "lucide-react";
import Link from "next/link";

type SubmissionStatus = "accepted" | "error";

const statusConfig = {
  accepted: {
    Icon: CheckIcon,
    label: "ACCEPTED",
    description: "All 15 test cases passed within limits.",
    iconBg: "bg-green-100",
    iconColor: "text-green-600",
    textColor: "text-green-600",
  },
  error: {
    Icon: XIcon,
    label: "ERROR",
    description: "Runtime error on test case 3.",
    iconBg: "bg-red-100",
    iconColor: "text-red-600",
    textColor: "text-red-600",
  },
} as const;

const testCases = [
  { id: 1, time: "0.002s", memory: "4.1 MB", passed: true },
  { id: 2, time: "0.003s", memory: "4.1 MB", passed: true },
  { id: 3, time: "0.001s", memory: "4.0 MB", passed: true },
];

export default function SubmissionPage() {
  const status: SubmissionStatus = "accepted";
  const { Icon, label, description, iconBg, iconColor, textColor } = statusConfig[status];

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
                {/* TODO: Select the next one problem.... */}
                <Button className="flex flex-1 items-center justify-center gap-2">
                  Next Problem <ArrowRightIcon className="h-4 w-4" />
                </Button>
                <Button variant="outline" className="flex items-center gap-2">
                  <CodeIcon className="h-4 w-4" /> Code
                </Button>
              </div>
            </div>

            {/* Competition Impact */}
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
                    +50 <span className="text-base font-semibold text-neutral-600">pts</span>
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Right column: Problem Info + Test Cases */}
          <div className="flex flex-col gap-4">
            {/* Problem Info */}
            <div className="rounded-2xl bg-white p-5">
              <div className="flex gap-2">
                <Badge variant="yellow">Medium</Badge>
                <Badge variant="red">50 Points</Badge>
              </div>
              <h3 className="mt-3 text-lg leading-tight font-black">
                Prime Factorization Optimization
              </h3>
              <p className="text-sm text-neutral-500">Problem ID: #PM-4092</p>

              <hr className="my-4 border-neutral-100" />

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-neutral-600">Submitted</p>
                  <p className="text-sm font-semibold">Oct 24, 2026 · 14:32</p>
                </div>
                <div>
                  <p className="text-xs text-neutral-600">Language</p>
                  <p className="text-sm font-semibold">C++ (GCC 9.2.0)</p>
                </div>
              </div>

              <div className="mt-4 grid grid-cols-2 gap-3">
                <div className="rounded-xl bg-neutral-50 p-4">
                  <p className="text-xs text-neutral-600">Runtime</p>
                  <p className="text-2xl font-black">0.012s</p>
                  <Badge variant="green" className="mt-1">Beats 94.2% of users</Badge>
                </div>
                <div className="rounded-xl bg-neutral-50 p-4">
                  <p className="text-xs text-neutral-600">Memory</p>
                  <p className="text-2xl font-black">4.2 MB</p>
                  <Badge variant="green" className="mt-1">Beats 88.5% of users</Badge>
                </div>
              </div>
            </div>

            {/* Test Case Details */}
            <div className="rounded-2xl bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <h4 className="font-bold">Test Case Details</h4>
                <span className="rounded-full bg-green-100 px-3 py-0.5 text-xs font-semibold text-green-700">
                  15/15 Passed
                </span>
              </div>
              <div className="space-y-2">
                {testCases.map((tc) => (
                  <div
                    key={tc.id}
                    className="flex items-center justify-between rounded-xl bg-neutral-50 px-4 py-3"
                  >
                    <div>
                      <p className="text-sm font-semibold">Test Case {tc.id}</p>
                      <p className="text-xs text-neutral-600">
                        Time {tc.time} &nbsp; Mem {tc.memory}
                      </p>
                    </div>
                    <span
                      className={`text-xs font-semibold ${tc.passed ? "text-green-600" : "text-red-600"}`}
                    >
                      {tc.passed ? "✓ Pass" : "✗ Fail"}
                    </span>
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
