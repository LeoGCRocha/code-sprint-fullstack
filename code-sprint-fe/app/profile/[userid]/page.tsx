import { Achievements } from "@/features/profile/Achievements";
import { Heatmap } from "@/features/profile/Heatmap";
import { ProblemCategories } from "@/features/profile/ProblemCategories";
import { Profile } from "@/features/profile/Profile";
import { StatsBar } from "@/features/profile/StatsBar";

export default function ProfilePage() {
  return (
    <div className="flex w-full flex-col gap-3 overflow-hidden p-3 md:grid md:grid-cols-[1fr_300px] md:items-start">
      <div className="flex flex-col gap-4">
        <Profile />
        <StatsBar />
        <Heatmap />
      </div>
      <aside className="flex flex-col gap-3">
        <ProblemCategories />
        <Achievements />
      </aside>
    </div>
  );
}
