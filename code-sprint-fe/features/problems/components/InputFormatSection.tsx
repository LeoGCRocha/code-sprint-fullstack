type InputFormatSectionProps = {
  lines: string[];
};

export function InputFormatSection({ lines }: InputFormatSectionProps) {
  return (
    <div className="flex flex-col gap-1">
      <h3 className="font-bold">Input Format</h3>
      {lines.map((line, i) => (
        <p key={i} className="text-sm text-neutral-700">{line}</p>
      ))}
    </div>
  );
}
