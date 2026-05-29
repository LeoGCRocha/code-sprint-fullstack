type ConstraintsListProps = {
  items: string[];
};

export function ConstraintsList({ items }: ConstraintsListProps) {
  return (
    <ul className="flex flex-col gap-1">
      {items.map((item, i) => (
        <li key={i} className="flex items-center gap-2">
          <span className="h-2 w-2 shrink-0 rounded-full bg-primary-500" />
          <span>{item}</span>
        </li>
      ))}
    </ul>
  );
}
