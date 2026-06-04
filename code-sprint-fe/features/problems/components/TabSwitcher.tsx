"use client";

import { useState } from "react";
import type { Problem } from "../types";
import { ConstraintsTab } from "./ConstraintsTab";
import { DescriptionTab } from "./DescriptionTab";
import { ExamplesTab } from "./ExamplesTab";

const tabs = ["Problem", "Examples", "Constraints"] as const;
type TabType = (typeof tabs)[number];

type TabSwitcherProps = {
  problem: Problem;
};

export function TabSwitcher({ problem }: TabSwitcherProps) {
  const [currentTab, setTab] = useState<TabType>("Problem");

  return (
    <div className="mt-4 inline-flex w-full flex-col">
      <div className="flex">
        {tabs.map((tab) => (
          <button
            key={tab}
            onClick={() => setTab(tab)}
            className={
              currentTab === tab
                ? "-mb-px flex-1 rounded-t-md border border-neutral-300 border-b-white bg-white px-4 pb-4 font-semibold"
                : "flex-1 px-4 pb-4 text-neutral-500"
            }
          >
            {tab}
          </button>
        ))}
      </div>
      <div className="rounded-b-md rounded-tr-md border border-neutral-300 bg-white p-4">
        {currentTab === "Problem" && <DescriptionTab problem={problem} />}
        {currentTab === "Examples" && <ExamplesTab problem={problem} />}
        {currentTab === "Constraints" && <ConstraintsTab problem={problem} />}
      </div>
    </div>
  );
}
