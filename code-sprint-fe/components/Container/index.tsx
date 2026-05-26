import { TrophyIcon } from "lucide-react";
import { ReactNode } from "react";

type Variant = "light" | "dark";

type ContainerProps = {
  children: ReactNode;
  icon?: ReactNode;
  title?: string;
  variant?: Variant;
};

const variantStyles = {
  light: {
    container: "bg-white",
    iconWrapper: "bg-primary-100 text-primary-500",
    title: "text-gray-900",
  },
  dark: {
    container: "bg-surface-dark",
    iconWrapper: "bg-surface-dark-elevated text-primary-500",
    title: "text-white",
  },
};

export function Container({ children, icon, title, variant = "light" }: ContainerProps) {
  const styles = variantStyles[variant];
  return (
    <div
      className={`${styles.container} mt-10 flex min-h-50 w-full max-w-md flex-col gap-0.5 rounded-2xl p-5`}
    >
      <div className={`${styles.iconWrapper} h-fit w-fit rounded-lg p-3`}>
        {icon ?? <TrophyIcon />}
      </div>
      <h2 className={`${styles.title} mb-4 text-3xl leading-tight font-black`}>{title}</h2>
      {children}
    </div>
  );
}
