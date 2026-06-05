import Link from "next/link";
import { Button } from "@/components/ui/Button";

export default function NotFound() {
  return (
    <div className="bg-background flex min-h-screen flex-col items-center justify-center gap-6 text-center">
      <p className="text-text-secondary text-8xl font-bold">404</p>
      <div className="flex flex-col gap-1.5">
        <h1 className="text-text-primary text-xl font-bold">Page not found</h1>
        <p className="text-text-secondary max-w-sm text-sm">
          The page you&apos;re looking for doesn&apos;t exist or has been moved.
        </p>
      </div>
      <Link href="/">
        <Button variant="outline">Go home</Button>
      </Link>
    </div>
  );
}
