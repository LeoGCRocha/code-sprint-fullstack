import { auth0 } from "@/lib/auth0";

export interface BeUser {
  id: string;
  handle: string;
  displayName: string;
  email: string;
  avatar: string | null;
  points: number;
  memberSince: string;
}

export async function getCurrentUser(): Promise<BeUser | null> {
  const session = await auth0.getSession();
  console.log("[getCurrentUser] session:", session ? "present" : "null");

  if (!session) return null;

  try {
    const res = await fetch(`${process.env.API_GATEWAY_URL}/users/me`, {
      headers: { Authorization: `Bearer ${session.tokenSet.accessToken}` },
      next: { revalidate: 60 },
    });

    if (!res.ok) {
      console.error("[getCurrentUser] status:", res.status, await res.text());
      return null;
    }

    return res.json();
  } catch (err) {
    console.error("[getCurrentUser] fetch failed:", err);
    return null;
  }
}
