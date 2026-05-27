interface StatItemProps {
  value: string;
  label: string;
}

export function StatItem({ value, label }: StatItemProps) {
  return (
    <div className="flex flex-col">
      <p className="leading-tight font-black">{value}</p>
      <small className="text-sm text-neutral-600">{label}</small>
    </div>
  );
}
