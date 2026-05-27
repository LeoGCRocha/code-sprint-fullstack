import { twMerge } from "tailwind-merge";

export type BadgeVariant = "blue" | "yellow" | "green" | "red";

const variantStyles: Record<BadgeVariant, string> = {
  blue: "bg-blue-100 text-blue-700",
  yellow: "bg-yellow-100 text-yellow-700",
  green: "bg-green-100 text-green-700",
  red: "bg-red-100 text-red-700",
};

interface BadgeProps {
  variant: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

export function Badge({ variant, children, className }: BadgeProps) {
  return (
    <span
      className={twMerge(
        "rounded-full px-3 py-1 text-sm font-semibold",
        variantStyles[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
