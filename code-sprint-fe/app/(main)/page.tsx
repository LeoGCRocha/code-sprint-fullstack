import { Hero } from "@/features/home/components/Hero";
import { PracticeModeCard } from "@/features/home/components/PracticeModeCard";
import { CompetitionModeCard } from "@/features/home/components/CompetitionModeCard";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center">
      <Hero />
      <div className="flex flex-col p-2.5 md:flex-row md:gap-5">
        <PracticeModeCard />
        <CompetitionModeCard />
      </div>
    </div>
  );
}
