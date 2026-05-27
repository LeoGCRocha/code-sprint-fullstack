import { TrophyIcon } from "lucide-react";
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";

export function CompetitionModeCard() {
  return (
    <Container variant="dark" title="Competition Mode" icon={<TrophyIcon />}>
      <p className="text-neutral-400">
        Test your limits against others globally. Participate in real-time contests, climb the
        leaderboards, and earn rating points.
      </p>

      {/* TODO: Load from backend */}
      <div className="mb-2 grid w-full grid-cols-2 gap-3">
        <div className="bg-surface-dark-elevated rounded-xl p-4 text-white">
          <small className="text-sm text-neutral-400">Active Contests</small>
          <h2 className="text-4xl leading-tight font-black">12</h2>
        </div>
        <div className="bg-surface-dark-elevated rounded-xl p-4 text-white">
          <small className="text-sm text-neutral-400">Global Rankers</small>
          <h2 className="text-4xl leading-tight font-black">150k+</h2>
        </div>
      </div>

      <Button className="w-full">View Competitions</Button>
    </Container>
  );
}
