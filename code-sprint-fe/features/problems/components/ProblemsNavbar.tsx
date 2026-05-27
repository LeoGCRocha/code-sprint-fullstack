import Link from "next/link";
import { Logo } from "@/components/layout/Logo";

const navLinks = [
  { label: "Problem List", href: "/problems", active: true },
  { label: "Competition Dashboard", href: "/competition" },
  { label: "Submit Solution", href: "/submit" },
  { label: "Profile and Settings", href: "/profile" },
];

export function ProblemsNavbar() {
  return (
    <nav className="flex items-center justify-between border-b border-neutral-200 bg-white px-6 py-4">
      <div className="flex items-center gap-8">
        <Logo />
        <ul className="hidden gap-6 md:flex">
          {navLinks.map((link) => (
            <li key={link.href}>
              <Link
                href={link.href}
                className={
                  link.active
                    ? "border-b-2 border-orange-400 pb-1 font-semibold"
                    : "text-neutral-500 hover:text-neutral-900"
                }
              >
                {link.label}
              </Link>
            </li>
          ))}
        </ul>
      </div>
      <div className="flex items-center gap-3">
        <span className="flex items-center gap-1 text-sm font-semibold">
          <span className="h-2 w-2 rounded-full bg-orange-400" />
          2,450 pts
        </span>
        <div className="flex h-8 w-8 items-center justify-center rounded-full bg-neutral-800 text-sm font-bold text-white">
          U
        </div>
      </div>
    </nav>
  );
}
