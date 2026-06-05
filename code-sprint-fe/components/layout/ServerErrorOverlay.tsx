"use client";

import { useEffect } from "react";
import { ServerCrashIcon } from "lucide-react";
import { Error } from "@/components/ui/Error";

const logoutUrl = `/auth/logout?returnTo=${process.env.NEXT_PUBLIC_APP_URL ?? "http://localhost:3000"}`;

export function ServerErrorOverlay() {
  useEffect(() => {
    window.location.href = logoutUrl;
  }, []);

  return (
    <Error
      icon={ServerCrashIcon}
      message="Couldn't reach our servers. Logging you out..."
    />
  );
}
