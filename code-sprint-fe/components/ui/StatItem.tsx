interface StatItemProps {
  value: string;
  label: string;
  isReversed?: boolean;
}

export function StatItem({ value, label, isReversed = false }: StatItemProps) {

  const blackComponent = "leading-tight font-black";
  const grayStyle = "text-sm text-neutral-600";

  return (
    <div className="flex flex-col">
      <p className={!isReversed ? blackComponent : grayStyle}>{value}</p>
      <small className={!isReversed ? grayStyle : blackComponent}>{label}</small>
    </div>
  );
}
