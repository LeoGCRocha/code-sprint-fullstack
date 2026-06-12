"use client";

import { useEffect, useRef, useState } from "react";
import type { Day } from "./index";

// Fixed cell geometry (px). The number of columns adapts to the layout width;
// the cells themselves never resize. GAP matches Tailwind gap-1 (0.25rem = 4px).
const CELL = 12;
const GAP = 4;

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

export function HeatmapGrid({ days }: { days: Day[] }) {
  const ref = useRef<HTMLDivElement>(null);
  const [cols, setCols] = useState(0);

  // Measure the container and derive how many fixed-size columns fit.
  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const measure = () => {
      const width = el.clientWidth;
      setCols(Math.max(1, Math.floor((width + GAP) / (CELL + GAP))));
    };

    measure();
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  // Clamp to the available pool, then take exactly `shown * 7` trailing days and
  // chunk into full columns of 7 — no null padding, so the grid is a perfect
  // rectangle (note: rows are not calendar weekdays, just sequential days).
  const maxCols = Math.floor(days.length / 7);
  const shown = cols > 0 ? Math.min(cols, maxCols) : 0;
  const cells = shown > 0 ? days.slice(-shown * 7) : [];

  const columns: Day[][] = [];
  for (let i = 0; i < shown; i++) {
    columns.push(cells.slice(i * 7, i * 7 + 7));
  }

  const total = cells.reduce((sum, d) => sum + d.count, 0);

  return (
    <div className="flex min-w-0 flex-col justify-center gap-1 rounded-2xl bg-white p-5">
      <h3 className="font-bold">Submission Activity</h3>
      <p className="mb-4 text-sm text-neutral-400">
        {total} submissions · Last {shown} weeks
      </p>

      {/* ref measures available width; cells are fixed, columns adapt, centered. */}
      <div ref={ref} className="flex w-full justify-center gap-1 overflow-hidden">
        {columns.map((col, wi) => (
          <div key={wi} className="flex flex-col gap-1">
            {col.map((day, di) => (
              <div
                key={di}
                title={`${day.date}: ${day.count} submissions`}
                style={{ width: CELL, height: CELL }}
                className={`rounded-sm ${getColorClass(day.count)}`}
              />
            ))}
          </div>
        ))}
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
