import { Achievements } from "@/features/profile/Achievements";
import { Heatmap } from "@/features/profile/Heatmap";
import { ProblemCategories } from "@/features/profile/ProblemCategories";
import { Profile } from "@/features/profile/Profile";
import { StatsBar } from "@/features/profile/StatsBar";
import { getCurrentUser } from "@/services/users";
import { redirect } from "next/navigation";

export default async function ProfilePage() {
  const result = await getCurrentUser();

  if (result.state !== "ok") redirect("/login");

  return (
    <div className="flex w-full flex-col gap-3 p-3 md:grid md:grid-cols-[1fr_300px] md:items-start">
      <div className="flex min-w-0 flex-col gap-4">
        <Profile user={result.user} />
        {/* TODO: Implement this */}
        <StatsBar />
        <Heatmap />
      </div>
      {/* // TODO: Implement achievements */}
      <aside className="flex flex-col gap-3">
        <ProblemCategories />
        <Achievements />
      </aside>
    </div>
  );
}
