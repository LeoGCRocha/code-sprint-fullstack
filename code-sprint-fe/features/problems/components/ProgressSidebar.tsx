import { StatItem } from "@/components/ui/StatItem";

const difficultyStats = [
  { label: "Easy", value: "12", color: "bg-orange-400" },
  { label: "Medium", value: "25", color: "bg-blue-400" },
  { label: "Hard", value: "5", color: "bg-blue-700" },
];

export function ProgressSidebar() {
  return (
    <div className="mb-2 rounded-2xl bg-white p-5 shadow-sm">
      <h3 className="mb-4 text-lg font-black">Your Progress</h3>
      <div className="flex flex-col gap-3">
        <StatItem value="#4,281" label="Global Rank" />
        <StatItem value="42/500" label="Problems Solved" />
        <div className="mt-2 flex flex-col gap-2">
          {difficultyStats.map((stat) => (
            <div key={stat.label} className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className={`h-2 w-2 rounded-full ${stat.color}`} />
                <span className="text-sm text-neutral-500">{stat.label}</span>
              </div>
              <span className="text-sm font-semibold">{stat.value}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
