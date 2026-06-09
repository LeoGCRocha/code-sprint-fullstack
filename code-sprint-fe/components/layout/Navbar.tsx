"use client";

import Link from "next/link";
import { Logo } from "@/components/layout/Logo";
import { MenuIcon, XIcon } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useCurrentUser } from "@/provider/UserProvider";
import { Button } from "../ui/Button";

const navLinks = [
  { label: "Problems", href: "/problems" },
  { label: "Competitions", href: "/competitions" },
  { label: "Profile", href: "/profile/me" },
] as const;

type NavHref = (typeof navLinks)[number]["href"];

interface NavbarProps {
  activeHref?: NavHref;
}

function UserMenu({ displayName, avatar }: { displayName: string; avatar: string | null }) {
  const [open, setOpen] = useState(false);
  const [imgLoaded, setImgLoaded] = useState(false);
  const [imgFailed, setImgFailed] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const imgRef = useRef<HTMLImageElement>(null);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Cached images can finish loading before React attaches onLoad — check the
  // element's complete state on mount so the photo still reveals.
  useEffect(() => {
    const img = imgRef.current;
    if (img?.complete) {
      if (img.naturalWidth > 0) setImgLoaded(true);
      else setImgFailed(true);
    }
  }, [avatar]);

  const showImg = !!avatar && !imgFailed;

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((v) => !v)}
        aria-label="User menu"
        aria-expanded={open}
        className="bg-primary-100 text-primary-700 relative flex h-8 w-8 items-center justify-center overflow-hidden rounded-full text-sm font-bold transition-opacity hover:opacity-80"
      >
        {/* Initial sits underneath: shown until the photo loads, and on failure. */}
        <span aria-hidden={showImg && imgLoaded}>{displayName?.[0]?.toUpperCase() ?? "U"}</span>
        {showImg && (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            ref={imgRef}
            src={avatar}
            alt={displayName}
            onLoad={() => setImgLoaded(true)}
            onError={() => setImgFailed(true)}
            className={`absolute inset-0 h-full w-full object-cover transition-opacity duration-200 ${
              imgLoaded ? "opacity-100" : "opacity-0"
            }`}
          />
        )}
      </button>

      {open && (
        <div className="border-border bg-surface absolute top-full right-0 z-50 mt-2 w-44 rounded-xl border py-1 shadow-md">
          <div className="border-border border-b px-4 py-2.5">
            <p className="text-text-primary truncate text-sm font-semibold">{displayName}</p>
          </div>
          <Link
            href="/profile/me"
            onClick={() => setOpen(false)}
            className="text-text-secondary hover:text-text-primary block px-4 py-2 text-sm transition-colors hover:bg-neutral-50"
          >
            Profile
          </Link>
          <a
            href={`/auth/logout?returnTo=${process.env.NEXT_PUBLIC_APP_URL ?? "http://localhost:3000"}`}
            className="block px-4 py-2 text-sm text-red-600 transition-colors hover:bg-red-50"
          >
            Log out
          </a>
        </div>
      )}
    </div>
  );
}

export function Navbar({ activeHref }: NavbarProps) {
  const [isOpen, setIsOpen] = useState(false);
  const user = useCurrentUser();

  if (!user) {
    return (
      <nav className="border-border bg-surface flex w-full items-center justify-between border-b px-6 py-4">
        <Logo />
        <Link href="/login">
          <Button variant="outline" size="sm">
            Log In
          </Button>
        </Link>
      </nav>
    );
  }

  return (
    <header>
      <nav className="border-border bg-surface flex items-center justify-between border-b px-6 py-4">
        <div className="flex items-center gap-8">
          <Logo />
          <ul className="hidden gap-6 md:flex">
            {navLinks.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  className={
                    link.href === activeHref
                      ? "text-text-primary font-semibold"
                      : "text-text-secondary hover:text-text-primary transition-colors"
                  }
                >
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-text-secondary hidden text-sm font-semibold md:block">
            2,450 pts
          </span>
          <UserMenu displayName={user.displayName} avatar={user.avatar} />
          <button
            className="flex items-center justify-center md:hidden"
            onClick={() => setIsOpen(!isOpen)}
            aria-label={isOpen ? "Close menu" : "Open menu"}
          >
            {isOpen ? <XIcon className="h-5 w-5" /> : <MenuIcon className="h-5 w-5" />}
          </button>
        </div>
      </nav>

      {isOpen && (
        <div className="border-border bg-surface flex flex-col border-b px-6 py-3 md:hidden">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              onClick={() => setIsOpen(false)}
              className={
                link.href === activeHref
                  ? "text-text-primary py-2.5 font-semibold"
                  : "text-text-secondary hover:text-text-primary py-2.5 transition-colors"
              }
            >
              {link.label}
            </Link>
          ))}
          <a
            href={`/auth/logout?returnTo=${process.env.NEXT_PUBLIC_APP_URL ?? "http://localhost:3000"}`}
            className="border-border mt-1 border-t py-2.5 pt-3 text-sm text-red-600 transition-colors"
          >
            Log out
          </a>
        </div>
      )}
    </header>
  );
}
