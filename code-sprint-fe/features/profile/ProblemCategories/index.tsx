interface Category {
  label: string;
  percent: number;
}

const categories: Category[] = [
  { label: "Arrays & Hashing", percent: 95 },
  { label: "Dynamic Programming", percent: 72 },
  { label: "Graph Theory", percent: 58 },
  { label: "Binary Search", percent: 80 },
  { label: "Mathematical", percent: 65 },
];

export function ProblemCategories() {
  return (
    // TODO: Load from the backend
    <div className="flex flex-col gap-4 rounded-2xl bg-white p-5">
      <h3 className="font-bold">Problem Categories</h3>
      <div className="flex flex-col gap-3">
        {categories.map((cat) => (
          <div key={cat.label} className="flex flex-col gap-1">
            <div className="flex justify-between text-sm">
              <span className="text-neutral-700">{cat.label}</span>
              <span className="text-primary-600 font-semibold">{cat.percent}%</span>
            </div>
            <div className="h-2 w-full overflow-hidden rounded-full bg-neutral-100">
              <div
                className="bg-primary-600 h-full rounded-full"
                style={{ width: `${cat.percent}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
