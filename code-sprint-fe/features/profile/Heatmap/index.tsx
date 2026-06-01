type Day = { date: string; count: number };

const colors = [
  "bg-neutral-100",
  "bg-orange-100",
  "bg-orange-300",
  "bg-orange-500",
  "bg-orange-800",
];

function getColorClass(count: number): string {
  if (count === 0) return colors[0];
  if (count <= 2) return colors[1];
  if (count <= 5) return colors[2];
  if (count <= 9) return colors[3];
  return colors[4];
}

function generateDays(submissions: Record<string, number>): Day[] {
  const days: Day[] = [];
  const today = new Date();
  for (let i = 364; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(today.getDate() - i);
    const key = date.toISOString().split("T")[0];
    days.push({ date: key, count: submissions[key] ?? 0 });
  }
  return days;
}

function groupIntoWeeks(days: Day[]): (Day | null)[][] {
  const weeks: (Day | null)[][] = [];
  const firstDayOfWeek = new Date(days[0].date).getDay();
  let week: (Day | null)[] = Array(firstDayOfWeek).fill(null);

  for (const day of days) {
    week.push(day);
    if (week.length === 7) {
      weeks.push(week);
      week = [];
    }
  }
  if (week.length > 0) weeks.push(week);
  return weeks;
}

// TODO(BE): Replace mock data with GET /users/:id/submission-activity
const mockSubmissions: Record<string, number> = {
  "2026-05-01": 3,
  "2026-05-03": 7,
  "2026-05-05": 1,
  "2026-05-10": 5,
  "2026-05-12": 2,
  "2026-05-15": 9,
  "2026-05-20": 4,
  "2026-05-22": 11,
  "2026-05-25": 6,
  "2026-04-10": 2,
  "2026-04-15": 8,
  "2026-04-20": 3,
  "2026-03-05": 1,
  "2026-03-18": 5,
  "2026-03-25": 10,
};

export function Heatmap() {
  const days = generateDays(mockSubmissions);
  const weeks = groupIntoWeeks(days);
  const total = days.reduce((sum, d) => sum + d.count, 0);

  return (
    <div className="flex flex-col justify-center gap-1 rounded-2xl bg-white p-5">
      <h3 className="font-bold">Submission Activity</h3>
      <p className="mb-4 text-sm text-neutral-400">{total} submissions · Last year</p>

      <div className="overflow-x-auto md:overflow-visible">
        <div className="flex justify-center gap-1">
          {weeks.map((week, wi) => (
            <div key={wi} className="flex flex-col gap-1">
              {week.map((day, di) =>
                day === null ? (
                  <div key={di} className="h-3 w-3" />
                ) : (
                  <div
                    key={di}
                    title={`${day.date}: ${day.count} submissions`}
                    className={`h-3 w-3 rounded-sm ${getColorClass(day.count)}`}
                  />
                )
              )}
            </div>
          ))}
        </div>
      </div>

      <div className="mt-3 flex items-center gap-1 text-xs text-neutral-400">
        <span>Less</span>
        {colors.map((cls) => (
          <div key={cls} className={`h-3 w-3 rounded-sm ${cls}`} />
        ))}
        <span>More</span>
      </div>
    </div>
  );
}
