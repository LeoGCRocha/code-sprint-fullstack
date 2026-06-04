"use client";

import { Button } from "@/components/ui/Button";
import { FcGoogle } from "react-icons/fc";
import { FaGithub } from "react-icons/fa";
import Link from "next/link";

export function LoginCard() {
  return (
    <div className="flex w-full max-w-sm flex-col">
      <div className="border-border bg-surface mt-10 rounded-2xl border p-8 shadow-sm">
        <div className="mb-7">
          <h1 className="text-text-primary text-xl font-bold">Welcome back</h1>
          <p className="text-text-secondary mt-1.5 text-sm">Log in to continue practicing</p>
        </div>

        <div className="flex flex-col gap-3">
          <a href="/auth/login?connection=google-oauth2" className="w-full">
            <Button variant="outline" className="flex w-full items-center justify-center gap-2">
              <FcGoogle size={18} />
              Continue with Google
            </Button>
          </a>
          <a href="/auth/login?connection=github" className="w-full">
            <Button variant="outline" className="flex w-full items-center justify-center gap-2">
              <FaGithub size={18} />
              Continue with GitHub
            </Button>
          </a>
        </div>
      </div>

      <p className="text-text-secondary mt-5 text-center text-xs">
        By signing in, you agree to our{" "}
        <Link
          href="/terms"
          className="hover:text-text-primary underline underline-offset-2 transition-colors"
        >
          Terms of Service
        </Link>{" "}
        and{" "}
        <Link
          href="/privacy"
          className="hover:text-text-primary underline underline-offset-2 transition-colors"
        >
          Privacy Policy
        </Link>
      </p>
    </div>
  );
}
