"use client";

import { useState } from "react";

const tabs = ["Description", "Examples", "Contraints"];

type TabsTypes = "Description" | "Examples" | "Contraints";

export function TabSwitcher() {
  const [currentTab, setTab] = useState<TabsTypes>("Description");

  return (
    <div className="mt-4 inline-flex w-full flex-col">
      <div className="flex">
        {tabs.map((tab) => (
          <button
            key={tab}
            onClick={() => setTab(tab as TabsTypes)}
            className={
              currentTab == tab
                ? "-mb-px flex-1 rounded-t-md border border-neutral-300 border-b-white bg-white px-4 pb-4 font-semibold"
                : "flex-1 px-4 pb-4 text-neutral-500"
            }
          >
            {tab}
          </button>
        ))}
      </div>
      <div className="rounded-b-md rounded-tr-md border border-neutral-300 bg-white p-4">
        CONTENT
      </div>
    </div>
  );
}
