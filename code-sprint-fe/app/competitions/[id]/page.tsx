// TODO(BE): All data on this page is hardcoded — replace with real API:
// - Fetch competition by ID from GET /competitions/:id
// - CurrentCompetition: timer should count down from BE-provided endTime (needs client component + Date diff)
// - My Status: fetch from GET /competitions/:id/me — rank, pts, problems solved/active/locked
// - "Continue Problem" button target should come from the active problem slug in user status
// - Live Standings: fetch from GET /competitions/:id/standings?page=1, paginate on "Load More"
// - Highlight current user row by matching user ID from auth session
import { CurrentCompetition } from "@/features/competitions/CurrentCompetition";
import { Button } from "@/components/ui/Button";
import { ArrowRightIcon } from "lucide-react";

const problems = [
  { id: "A", status: "solved" },
  { id: "B", status: "solved" },
  { id: "C", status: "active" },
  { id: "D", status: "locked" },
] as const;

type ProblemStatus = (typeof problems)[number]["status"];

const problemStatusStyles: Record<
  ProblemStatus,
  { pill: string; label: string; labelColor: string }
> = {
  solved: {
    pill: "bg-green-100 text-green-700 border border-green-200",
    label: "Solved",
    labelColor: "text-green-600",
  },
  active: {
    pill: "bg-orange-100 text-orange-700 border border-orange-200",
    label: "Active",
    labelColor: "text-orange-600",
  },
  locked: {
    pill: "bg-neutral-100 text-neutral-400 border border-neutral-200",
    label: "Locked",
    labelColor: "text-neutral-400",
  },
};

const standings = [
  {
    rank: 1,
    initial: "N",
    name: "neural_ninja",
    solved: "4/4",
    pts: 820,
    avatarBg: "bg-green-500",
    isYou: false,
  },
  {
    rank: 2,
    initial: "B",
    name: "byte_wizard",
    solved: "4/4",
    pts: 780,
    avatarBg: "bg-blue-500",
    isYou: false,
  },
  {
    rank: 3,
    initial: "A",
    name: "algo_queen",
    solved: "3/4",
    pts: 640,
    avatarBg: "bg-orange-500",
    isYou: false,
  },
  {
    rank: 47,
    initial: "Y",
    name: "you",
    solved: "2/4",
    pts: 350,
    avatarBg: "bg-orange-400",
    isYou: true,
  },
];

export default function CompetitionsPage() {
  return (
    <div className="bg-background min-h-screen px-4 py-4">
      <div className="space-y- md:grid md:grid-cols-[1fr_360px] md:gap-4 md:space-y-0">
        <CurrentCompetition />

        {/* My Status */}
        <div className="rounded-2xl bg-white p-5">
          <h2 className="mb-3 font-bold">My Status</h2>

          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-neutral-900 text-lg font-black text-white">
              #47
            </div>
            <div className="flex-1">
              <p className="text-xs text-neutral-400">Current Rank</p>
              <p className="text-2xl font-black">350 pts</p>
            </div>
          </div>

          <div className="mt-4">
            <div className="mb-1 flex items-center justify-between">
              <span className="text-xs text-neutral-500">Problems Solved</span>
              <span className="text-xs font-semibold">2/4</span>
            </div>
            <div className="h-2 w-full overflow-hidden rounded-full bg-neutral-100">
              <div className="h-full w-1/2 rounded-full bg-green-500" />
            </div>
          </div>

          <div className="mt-4 flex gap-2">
            {problems.map((p) => {
              const s = problemStatusStyles[p.status];
              return (
                <div key={p.id} className="flex flex-1 flex-col items-center gap-1">
                  <span
                    className={`flex h-9 w-9 items-center justify-center rounded-full text-sm font-black ${s.pill}`}
                  >
                    {p.id}
                  </span>
                  <span className={`text-xs font-semibold ${s.labelColor}`}>{s.label}</span>
                </div>
              );
            })}
          </div>

          <Button className="mt-4 flex w-full items-center justify-center gap-2 rounded-xl bg-neutral-900 text-white hover:bg-neutral-800">
            Continue Problem C <ArrowRightIcon className="h-4 w-4" />
          </Button>
        </div>

        {/* Live Standings */}
        <div className="rounded-2xl bg-white p-5">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="font-bold">Live Standings</h2>
            <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-2 py-0.5 text-xs font-semibold text-green-700">
              <span className="h-1.5 w-1.5 rounded-full bg-green-500" />
              Active
            </span>
          </div>

          <div className="mb-2 grid grid-cols-[1.5rem_1fr_auto_auto] gap-x-3 px-1 text-xs font-semibold text-neutral-400">
            <span>#</span>
            <span>Participant</span>
            <span>Solved</span>
            <span>Pts</span>
          </div>

          <div className="space-y-2">
            {standings.map((s) => (
              <div
                key={s.rank}
                className={`grid grid-cols-[1.5rem_1fr_auto_auto] items-center gap-x-3 rounded-xl px-3 py-2.5 ${
                  s.isYou ? "bg-orange-50" : "bg-neutral-50"
                }`}
              >
                <span className="text-sm font-bold text-neutral-500">{s.rank}</span>
                <div className="flex items-center gap-2">
                  <span
                    className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-black text-white ${s.avatarBg}`}
                  >
                    {s.initial}
                  </span>
                  <span className={`text-sm font-semibold ${s.isYou ? "text-orange-600" : ""}`}>
                    {s.name}
                  </span>
                </div>
                <span className="text-sm text-neutral-500">{s.solved}</span>
                <span className="text-sm font-bold">{s.pts}</span>
              </div>
            ))}
          </div>

          <button className="mt-3 w-full rounded-xl border border-neutral-200 py-2.5 text-sm font-semibold text-neutral-600 hover:bg-neutral-50">
            Load More Participants
          </button>
        </div>

        {/* Footer */}
        <footer className="pb-6 text-center text-xs text-neutral-400">
          <p>© 2026 Code Sprint Platform</p>
          <p className="mt-1 flex justify-center gap-3">
            <span className="cursor-pointer hover:text-neutral-600">Terms</span>
            <span className="cursor-pointer hover:text-neutral-600">Privacy</span>
            <span className="cursor-pointer hover:text-neutral-600">Help</span>
          </p>
        </footer>
      </div>
    </div>
  );
}
