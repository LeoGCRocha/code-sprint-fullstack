import type { Example } from "../types";

type ExampleCardProps = {
  example: Example;
  index: number;
};

export function ExampleCard({ example, index }: ExampleCardProps) {
  return (
    <div className="flex flex-col gap-2 rounded-xl border border-neutral-200 bg-white p-4">
      <h3 className="font-bold">Example {index}</h3>
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold text-neutral-500">Input</span>
        <pre className="rounded-lg bg-neutral-100 p-2 text-sm whitespace-pre-wrap">{example.input}</pre>
      </div>
      <div className="flex flex-col gap-1">
        <span className="text-xs font-semibold text-neutral-500">Output</span>
        <pre className="rounded-lg bg-neutral-100 p-2 text-sm whitespace-pre-wrap">{example.output}</pre>
      </div>
      {example.explanation && (
        <div className="flex flex-col gap-1">
          <span className="text-xs font-semibold text-neutral-500">Explanation</span>
          <p className="text-sm text-neutral-700">{example.explanation}</p>
        </div>
      )}
    </div>
  );
}
