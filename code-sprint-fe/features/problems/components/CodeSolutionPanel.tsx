"use client";

import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { ChevronDownIcon, SendIcon } from "lucide-react";
import { CodeSolution } from "./CodeSolution";

const languages = [
  { label: "Python 3", value: "python3", monacoId: "python" },
  { label: "JavaScript", value: "javascript", monacoId: "javascript" },
  { label: "C++", value: "cpp", monacoId: "cpp" },
];

interface CodeSolutionPanelProps {
  fillHeight?: boolean;
}

export function CodeSolutionPanel({ fillHeight = false }: CodeSolutionPanelProps) {
  const [language, setLanguage] = useState(languages[0]);
  const [code, setCode] = useState("");

  return (
    <div className={fillHeight ? "flex h-full flex-col" : "flex flex-col"}>
      <div className="mt-3 flex items-center justify-between rounded-t-xl bg-neutral-800 px-4 py-2">
        <span className="text-sm font-semibold text-white">{language.label}</span>
        <div className="relative">
          <select
            value={language.value}
            onChange={(e) => setLanguage(languages.find((l) => l.value === e.target.value)!)}
            className="cursor-pointer appearance-none rounded-lg bg-neutral-600 px-3 py-1 text-sm text-white"
          >
            {languages.map((l) => (
              <option key={l.value} value={l.value}>{l.label}</option>
            ))}
          </select>
          <ChevronDownIcon className="pointer-events-none absolute top-1/2 right-2 h-3 w-3 -translate-y-1/2 text-white" />
        </div>
      </div>

      {fillHeight ? (
        <div className="min-h-0 flex-1">
          <CodeSolution
            language={language.monacoId}
            value={code}
            onChange={setCode}
            height="100%"
          />
        </div>
      ) : (
        <CodeSolution
          language={language.monacoId}
          value={code}
          onChange={setCode}
        />
      )}

      <Button className="mt-2 flex shrink-0 items-center justify-center gap-2 p-4">
        <SendIcon /> Submit Solution
      </Button>
    </div>
  );
}
