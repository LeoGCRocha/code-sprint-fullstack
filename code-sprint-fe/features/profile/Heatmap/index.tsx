import type { SubmissionActivity } from "@/services/activity";
import { HeatmapGrid } from "./HeatmapGrid";

export type Day = { date: string; count: number };

// Pool of trailing days the server ships to the client. The client grid renders
// as many trailing WEEKS as fit the layout at a fixed cell size, so this is just
// the upper bound (about 1 year), enough to fill very wide layouts.
const POOL_WEEKS = 53;
const POOL_DAYS = POOL_WEEKS * 7;

export function generateDays(submissions: SubmissionActivity): Day[] {
  const days: Day[] = [];
  const today = new Date();
  for (let i = POOL_DAYS - 1; i >= 0; i--) {
    const date = new Date(today);
    date.setDate(today.getDate() - i);
    const key = date.toISOString().split("T")[0];
    days.push({ date: key, count: submissions[key] ?? 0 });
  }
  return days;
}

export function Heatmap({ activity }: { activity: SubmissionActivity }) {
  const days = generateDays(activity);

  return <HeatmapGrid days={days} />;
}
