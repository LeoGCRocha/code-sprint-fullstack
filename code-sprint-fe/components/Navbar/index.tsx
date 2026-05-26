import Link from "next/link";
import { Button } from "@/components/Button";
import { Logo } from "@/components/Logo";

export function Navbar() {
  return (
    <nav className="flex w-full justify-between py-2 pr-1 pl-1 md:px-6">
      <Logo />
      <Link href="/login">
        <Button variant="outline">Log In</Button>
      </Link>
    </nav>
  );
}
