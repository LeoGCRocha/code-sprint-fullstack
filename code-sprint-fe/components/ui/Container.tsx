import { ReactNode } from "react";
import { twMerge } from "tailwind-merge";

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
      className={twMerge("mt-10 flex min-h-50 w-full max-w-md flex-col gap-0.5 rounded-2xl p-5", styles.container)}
    >
      {icon && (
        <div className={twMerge("h-fit w-fit rounded-lg p-3", styles.iconWrapper)}>{icon}</div>
      )}
      {title && (
        <h2 className={twMerge("mb-4 text-3xl leading-tight font-black", styles.title)}>{title}</h2>
      )}
      {children}
    </div>
  );
}
