"use client";

import { createContext, useContext, type ReactNode } from "react";
import type { BeUser } from "@/services/users";

const UserContext = createContext<BeUser | null>(null);

export function UserProvider({
  children,
  initialUser,
}: {
  children: ReactNode;
  initialUser: BeUser | null;
}) {
  return <UserContext.Provider value={initialUser}>{children}</UserContext.Provider>;
}

export const useCurrentUser = () => useContext(UserContext);
