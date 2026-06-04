// TODO(BE): Replace all mock data with real API calls:
// - Fetch problem by slug from GET /problems/:slug
// - Fetch competition by id from GET /competitions/:id (name, endTime, scoring rules)
// - secondsRemaining derived from competition.endTime - Date.now()
// - My submissions from GET /submissions?problemSlug=:slug&competitionId=:id
// - Scoring (points, firstBlood bonus, speedBonus, penalty) from competition.scoring
import { notFound } from "next/navigation";
import Link from "next/link";
import { Badge } from "@/components/ui/Badge";
import { TabSwitcher } from "@/features/problems/components/TabSwitcher";
import { CompetitionProblemBar } from "@/features/competitions/CompetitionProblemBar";
import { problems } from "@/features/problems/data";
import { Button } from "@/components/ui/Button";
import { SendIcon } from "lucide-react";

const mockCompetition = {
  id: "1",
  name: "Corner Problems",
  secondsRemaining: 5078,
  scoring: {
    fullSolve: 100,
    firstBlood: 120,
    speedBonus: 10,
    wrongPenalty: -10,
  },
  navProblems: [
    { id: "A", status: "solved" as const, slug: "two-sum" },
    { id: "B", status: "active" as const, slug: "merge-intervals" },
    { id: "C", status: "locked" as const, slug: "word-break" },
    { id: "D", status: "locked" as const, slug: "longest-substring" },
  ],
};

const mockSubmissions = [
  { verdict: "Accepted", runtime: "0.42s", ago: "2m ago", color: "text-green-600" },
  { verdict: "Wrong Answer", runtime: "0.38s", ago: "8m ago", color: "text-red-500" },
  { verdict: "TLE", runtime: "2.0s", ago: "13m ago", color: "text-yellow-600" },
];

const difficultyVariant = { easy: "green", medium: "yellow", hard: "red" } as const;

export default async function CompetitionProblemPage({
  params,
}: {
  params: Promise<{ slug: string; id: string }>;
}) {
  const { slug, id } = await params;
  const problem = problems.find((p) => p.slug === slug);
  if (!problem) return notFound();

  const { scoring } = mockCompetition;

  return (
    <div className="bg-background min-h-screen pb-10">
      <CompetitionProblemBar
        competitionId={id}
        competitionName={mockCompetition.name}
        secondsRemaining={mockCompetition.secondsRemaining}
        problems={mockCompetition.navProblems}
        activeSlug={slug}
      />

      {/* Problem header */}
      <div className="mx-3 mt-3 rounded-2xl bg-white p-5">
        <div className="mb-3 flex items-center justify-between">
          <div className="font-mono text-sm font-bold text-neutral-500">
            ⏱ {Math.floor(mockCompetition.secondsRemaining / 60)}m remaining
          </div>
          <Link href={`/problems/${slug}/solution`}>
            <Button size="sm" className="flex items-center gap-1.5 rounded-xl">
              <SendIcon className="h-3.5 w-3.5" /> Submit Solution
            </Button>
          </Link>
        </div>

        <p className="mb-1 text-xs text-neutral-400">
          {mockCompetition.name} · Problem ID: CP-{problem.id.padStart(4, "0")}
        </p>

        <div className="mb-2 flex flex-wrap gap-1.5">
          <Badge variant={difficultyVariant[problem.difficulty]}>
            {problem.difficulty.charAt(0).toUpperCase() + problem.difficulty.slice(1)}
          </Badge>
          <Badge variant="red">{problem.points} pts</Badge>
          <Badge variant="blue">Competition</Badge>
        </div>

        <h1 className="text-2xl font-black leading-tight">{problem.title}</h1>

        <div className="mt-4 grid grid-cols-4 divide-x divide-neutral-100 text-center">
          <div className="flex flex-col gap-0.5 px-2">
            <span className="text-sm font-bold">{problem.estimatedTime}</span>
            <span className="text-xs text-neutral-400">Time</span>
          </div>
          <div className="flex flex-col gap-0.5 px-2">
            <span className="text-sm font-bold">{problem.points}</span>
            <span className="text-xs text-neutral-400">Points</span>
          </div>
          <div className="flex flex-col gap-0.5 px-2">
            <span className="text-sm font-bold">{(problem.solvedCount / 1000).toFixed(1)}k</span>
            <span className="text-xs text-neutral-400">Solved</span>
          </div>
          <div className="flex flex-col gap-0.5 px-2">
            <span className="text-sm font-bold capitalize">{problem.difficulty}</span>
            <span className="text-xs text-neutral-400">Level</span>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="mx-3 mt-3">
        <TabSwitcher problem={problem} />
      </div>

      {/* Scoring */}
      <div className="mx-3 mt-3 rounded-2xl bg-neutral-900 p-5">
        <h3 className="mb-3 font-bold text-white">Scoring</h3>
        <div className="grid grid-cols-3 gap-3">
          <div className="flex flex-col gap-0.5">
            <span className="text-lg font-black text-primary-400">{scoring.fullSolve} pts</span>
            <span className="text-xs text-neutral-400">Full Solve</span>
          </div>
          <div className="flex flex-col gap-0.5">
            <span className="text-lg font-black text-green-400">{scoring.firstBlood} pts</span>
            <span className="text-xs text-neutral-400">First Blood</span>
          </div>
          <div className="flex flex-col gap-0.5">
            <span className="text-lg font-black text-green-400">+{scoring.speedBonus} pts</span>
            <span className="text-xs text-neutral-400">Speed Bonus</span>
          </div>
        </div>
        <p className="mt-3 text-xs text-neutral-500">
          Penalty: {scoring.wrongPenalty} pts per wrong submission
        </p>
      </div>

      {/* Limits */}
      <div className="mx-3 mt-3 rounded-2xl bg-white p-5">
        <h3 className="mb-3 font-bold">Limits</h3>
        <div className="grid grid-cols-3 gap-3">
          <div className="flex flex-col gap-0.5">
            <span className="text-sm font-bold">2 seconds</span>
            <span className="text-xs text-neutral-400">Time Limit</span>
          </div>
          <div className="flex flex-col gap-0.5">
            <span className="text-sm font-bold">256 MB</span>
            <span className="text-xs text-neutral-400">Memory</span>
          </div>
          <div className="flex flex-col gap-0.5">
            <span className="text-sm font-bold">64 MB</span>
            <span className="text-xs text-neutral-400">Stack</span>
          </div>
        </div>
      </div>

      {/* My Submissions */}
      <div className="mx-3 mt-3 rounded-2xl bg-white p-5">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="font-bold">My Submissions</h3>
          <Link href={`/submissions`} className="text-sm text-primary-600 hover:underline">
            View All
          </Link>
        </div>
        <div className="flex flex-col gap-2">
          {mockSubmissions.map((s, i) => (
            <div key={i} className="flex items-center justify-between rounded-xl bg-neutral-50 px-4 py-2.5">
              <span className={`text-sm font-semibold ${s.color}`}>{s.verdict}</span>
              <div className="flex items-center gap-3 text-xs text-neutral-400">
                <span>{s.runtime}</span>
                <span>{s.ago}</span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
