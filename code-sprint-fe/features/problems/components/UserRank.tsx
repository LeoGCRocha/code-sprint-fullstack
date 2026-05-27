import { StatItem } from "@/components/ui/StatItem";

export function UserRank() {
  return (
    <div className="m-2 flex flex-col gap-2 rounded-lg bg-white p-2">
      <h2 className="text-2xl font-black">Your Progress</h2>
      <div className="flex flex-row justify-between">
        <StatItem value="#3,321" label="Global rank" />
        <StatItem value="42/500" label="Solved" />
        <StatItem value="7 Days" label="Streak" />
      </div>
      <div className="flex flex-col items-center">
        <h2 className="text-xl leading-tight font-black">Points</h2>
        <p className="text-sm text-neutral-600">38.4k</p>
      </div>
    </div>
  );
}
