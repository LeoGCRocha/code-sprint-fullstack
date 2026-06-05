import { auth0 } from "@/lib/auth0";
import { mockUser } from "@/mocks/data/user";

export interface BeUser {
  id: string;
  handle: string;
  displayName: string;
  email: string;
  avatar: string | null;
  points: number;
  memberSince: string;
}

export type UserResult =
  | { state: "unauthenticated" }
  | { state: "error" }
  | { state: "ok"; user: BeUser };

export async function getCurrentUser(): Promise<UserResult> {
  if (process.env.USE_MOCKS === "true") {
    return { state: "ok", user: mockUser };
  }

  const session = await auth0.getSession();

  if (!session) return { state: "unauthenticated" };

  try {
    const res = await fetch(`${process.env.API_GATEWAY_URL}/users/me`, {
      headers: { Authorization: `Bearer ${session.tokenSet.accessToken}` },
      next: { revalidate: 60 },
    });

    if (!res.ok) {
      console.error("[getCurrentUser] status:", res.status, await res.text());
      return { state: "error" };
    }

    return { state: "ok", user: await res.json() };
  } catch (err) {
    console.error("[getCurrentUser] fetch failed:", err);
    return { state: "error" };
  }
}
