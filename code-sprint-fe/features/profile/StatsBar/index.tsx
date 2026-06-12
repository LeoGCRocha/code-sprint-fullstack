interface Stat {
  value: string;
  label: string;
  dark?: boolean;
}

export type StatsBarProps = {
  stats: Stat[];
};

export function StatsBar({ stats }: StatsBarProps) {
  return (
    <div className="flex rounded-2xl bg-white">
      {stats.map((stat, i) => (
        <div
          key={stat.label}
          className={`flex flex-1 flex-col items-center justify-center gap-0.5 py-4 ${
            i === 0 ? "rounded-l-2xl" : ""
          } ${i === stats.length - 1 ? "rounded-r-2xl" : ""} ${
            stat.dark ? "bg-neutral-900 text-white" : ""
          }`}
        >
          <span className={`text-xl font-black leading-tight ${stat.dark ? "text-white" : "text-neutral-900"}`}>
            {stat.value}
          </span>
          <span className={`text-xs ${stat.dark ? "text-neutral-400" : "text-neutral-500"}`}>
            {stat.label}
          </span>
        </div>
      ))}
    </div>
  );
}
