import { Hero } from "@/components/Hero";
import { Button } from "@/components/Button";
import { Container } from "@/components/Container";
import { DumbbellIcon, TrophyIcon } from "lucide-react";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center">
      <Hero />

      <div className="flex flex-col p-2.5 md:flex-row md:gap-5">
        {/* Pratice Mode Container */}
        <Container variant="light" title="Pratice Mode" icon={<DumbbellIcon />}>
          <p className="text-text-secondary">
            Hone your skills at your own pace. Browse thousands of problems categorized by
            difficulty and topic. Perfect for learning and interview prep.
          </p>
          <div className="m-1 grid grid-cols-2 gap-2 p-1">
            {["Mathematical", "Logical", "Geometrics", "Strings"].map((tag) => (
              <span
                key={tag}
                className="text-text-secondary rounded-xl bg-neutral-100 px-4 py-2 text-sm"
              >
                {tag}
              </span>
            ))}
          </div>
          <Button>Explore problems</Button>
        </Container>

        {/* Competitive Mode Container */}
        <Container variant="dark" title="Competition Mode" icon={<TrophyIcon />}>
          <p className="text-neutral-400">
            Test your limits against others globally. Participate in real-time contests, climb the
            leaderboards, and earn rating points.
          </p>

          {/* TODO: Load this from backend */}
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
      </div>
    </div>
  );
}
