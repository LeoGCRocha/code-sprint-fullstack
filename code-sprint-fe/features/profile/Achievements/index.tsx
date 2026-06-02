import { Flame, Zap, CheckCircle, Timer, Star, Shield } from "lucide-react";

interface Achievement {
  icon: React.ReactNode;
  label: string;
  bg: string;
  iconColor: string;
  dark?: boolean;
}

const achievements: Achievement[] = [
  { icon: <Flame size={24} />, label: "First Blood", bg: "bg-red-50", iconColor: "text-red-500" },
  { icon: <Zap size={24} />, label: "Speed Demon", bg: "bg-green-50", iconColor: "text-green-500" },
  { icon: <CheckCircle size={24} />, label: "100 Solved", bg: "bg-blue-50", iconColor: "text-blue-500" },
  { icon: <Timer size={24} />, label: "30 Streak", bg: "bg-orange-50", iconColor: "text-orange-500" },
  { icon: <Star size={24} />, label: "Top 10", bg: "bg-yellow-50", iconColor: "text-yellow-500" },
  { icon: <Shield size={24} />, label: "Veteran", bg: "bg-neutral-900", iconColor: "text-white", dark: true },
];

const TOTAL_EARNED = 18;

export function Achievements() {
  return (
    // TODO: Load from the backend
    <div className="flex flex-col gap-4 rounded-2xl bg-white p-5">
      <div className="flex items-center justify-between">
        <h3 className="font-bold">Achievements</h3>
        <span className="text-sm text-neutral-400">{TOTAL_EARNED} earned</span>
      </div>
      <div className="grid grid-cols-3 gap-2">
        {achievements.map((a) => (
          <div
            key={a.label}
            className={`flex flex-col items-center justify-center gap-1.5 rounded-2xl p-4 ${a.bg}`}
          >
            <span className={a.iconColor}>{a.icon}</span>
            <span className={`text-center text-xs font-semibold ${a.dark ? "text-white" : "text-neutral-700"}`}>
              {a.label}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
