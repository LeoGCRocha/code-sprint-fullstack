"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ChevronLeftIcon } from "lucide-react";

type ProblemNav = {
  id: string;
  status: "solved" | "active" | "locked";
  slug: string;
};

interface CompetitionProblemBarProps {
  competitionId: string;
  competitionName: string;
  // seconds until competition ends
  secondsRemaining: number;
  problems: ProblemNav[];
  activeSlug: string;
}

function useCountdown(initialSeconds: number) {
  const [seconds, setSeconds] = useState(initialSeconds);

  useEffect(() => {
    if (seconds <= 0) return;
    const id = setInterval(() => setSeconds((s) => s - 1), 1000);
    return () => clearInterval(id);
  }, [seconds]);

  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  const s = seconds % 60;
  return `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}

const pillStyles: Record<ProblemNav["status"], string> = {
  solved: "bg-green-100 text-green-700 border border-green-200",
  active: "bg-neutral-900 text-white",
  locked: "bg-neutral-100 text-neutral-400 border border-neutral-200",
};

export function CompetitionProblemBar({
  competitionId,
  competitionName,
  secondsRemaining,
  problems,
  activeSlug,
}: CompetitionProblemBarProps) {
  const time = useCountdown(secondsRemaining);

  return (
    <div className="flex shrink-0 items-center justify-between border-b border-neutral-200 bg-white px-4 py-2">
      <div className="flex items-center gap-3">
        <Link
          href={`/competitions/${competitionId}`}
          className="flex items-center gap-1 text-sm text-neutral-500 hover:text-neutral-900"
        >
          <ChevronLeftIcon className="h-4 w-4" />
          <span className="hidden sm:inline">{competitionName}</span>
        </Link>

        <div className="flex gap-1.5">
          {problems.map((p) => (
            <Link
              key={p.id}
              href={`/problems/${p.slug}/competition/${competitionId}`}
              className={`flex h-8 w-8 items-center justify-center rounded-full text-sm font-black transition-colors ${pillStyles[p.status]} ${p.slug === activeSlug ? "ring-2 ring-neutral-400 ring-offset-1" : ""}`}
            >
              {p.id}
            </Link>
          ))}
        </div>
      </div>

      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 animate-pulse rounded-full bg-red-500" />
        <span className="font-mono text-sm font-bold tabular-nums text-neutral-700">{time}</span>
      </div>
    </div>
  );
}
