import { Achievements } from "@/features/profile/Achievements";
import { Heatmap } from "@/features/profile/Heatmap";
import { ProblemCategories } from "@/features/profile/ProblemCategories";
import { Profile } from "@/features/profile/Profile";
import { StatsBar } from "@/features/profile/StatsBar";
import { getSubmissionActivity, type SubmissionActivity } from "@/services/activity";
import { getCurrentUser } from "@/services/users";
import { redirect } from "next/navigation";

function formatNumber(value: number): string {
  return new Intl.NumberFormat("en-US").format(value);
}

function calculateCurrentStreak(activity: SubmissionActivity): number {
  let streak = 0;
  const cursor = new Date();

  while (true) {
    const key = cursor.toISOString().split("T")[0];
    if ((activity[key] ?? 0) === 0) return streak;

    streak++;
    cursor.setUTCDate(cursor.getUTCDate() - 1);
  }
}

function buildProfileStats(userPoints: number, activity: SubmissionActivity) {
  const totalSubmissions = Object.values(activity).reduce((sum, count) => sum + count, 0);
  const activeDays = Object.values(activity).filter((count) => count > 0).length;
  const currentStreak = calculateCurrentStreak(activity);

  return [
    { value: formatNumber(totalSubmissions), label: "Submissions" },
    { value: formatNumber(userPoints), label: "Points" },
    { value: `${currentStreak}d`, label: "Streak" },
    { value: formatNumber(activeDays), label: "Active Days", dark: true },
  ];
}

export default async function ProfilePage() {
  const result = await getCurrentUser();

  if (result.state !== "ok") redirect("/login");

  const activity = await getSubmissionActivity();
  const stats = buildProfileStats(result.user.points, activity);

  return (
    <div className="flex w-full flex-col gap-3 p-3 md:grid md:grid-cols-[1fr_300px] md:items-start">
      <div className="flex min-w-0 flex-col gap-4">
        <Profile user={result.user} />
        <StatsBar stats={stats} />
        <Heatmap activity={activity} />
      </div>
      {/* // TODO: Implement achievements */}
      <aside className="flex flex-col gap-3">
        <ProblemCategories />
        <Achievements />
      </aside>
    </div>
  );
}
