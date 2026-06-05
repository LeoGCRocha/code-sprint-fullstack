import { type LucideIcon, XIcon } from "lucide-react";

type ErrorProps = {
  message: string;
  icon?: LucideIcon;
  onClose?: () => void;
};

export function Error({ message, icon: Icon = XIcon, onClose }: ErrorProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="bg-surface flex flex-col items-center gap-4 rounded-2xl p-8 shadow-xl">
        <Icon className="h-16 w-16 text-red-500" />
        <p className="text-text-primary text-lg font-semibold">Something went wrong</p>
        <span className="text-text-secondary text-sm">{message}</span>
        {onClose && (
          <button
            onClick={onClose}
            className="text-text-secondary hover:text-text-primary text-sm underline underline-offset-2 transition-colors"
          >
            Dismiss
          </button>
        )}
      </div>
    </div>
  );
}
