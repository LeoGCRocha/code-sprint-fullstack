"use client";

import Link from "next/link";
import { Logo } from "@/components/layout/Logo";
import { MenuIcon, XIcon } from "lucide-react";
import { useState } from "react";

const navLinks = [
  { label: "Problem List", href: "/problems" },
  { label: "Competition Dashboard", href: "/competitions" },
  { label: "Submit Solution", href: "/submit" },
  { label: "Profile and Settings", href: "/profile" },
] as const;

type NavHref = (typeof navLinks)[number]["href"];

interface AppNavbarProps {
  activeHref?: NavHref;
}

export function AppNavbar({ activeHref }: AppNavbarProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      <nav className="flex items-center justify-between border-b border-neutral-200 bg-white px-6 py-4">
        <div className="flex items-center gap-8">
          <Logo />
          <ul className="hidden gap-6 md:flex">
            {navLinks.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  className={
                    link.href === activeHref
                      ? "border-b-2 border-primary-400 pb-1 font-semibold"
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
            <span className="h-2 w-2 rounded-full bg-primary-400" />
            2,450 pts
          </span>
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-neutral-800 text-sm font-bold text-white">
            U
          </div>
          <button className="md:hidden" onClick={() => setIsOpen(!isOpen)}>
            {isOpen ? <XIcon className="h-5 w-5" /> : <MenuIcon className="h-5 w-5" />}
          </button>
        </div>
      </nav>

      {isOpen && (
        <div className="md:hidden border-b border-neutral-200 bg-white px-6 py-4 flex flex-col gap-4">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              onClick={() => setIsOpen(false)}
              className={
                link.href === activeHref
                  ? "font-semibold text-neutral-900"
                  : "text-neutral-500 hover:text-neutral-900"
              }
            >
              {link.label}
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
