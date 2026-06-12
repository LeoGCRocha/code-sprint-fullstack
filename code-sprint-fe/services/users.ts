import { auth0 } from "@/lib/auth0";
import { mockUser } from "@/mocks/data/user";
import { cookies } from "next/headers";

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
    const jar = await cookies();
    if (jar.get("mock_logged_out")?.value === "true") return { state: "unauthenticated" };
    return { state: "ok", user: mockUser };
  }

  const session = await auth0.getSession();

  if (!session) return { state: "unauthenticated" };

  try {
    // Gateway validates the Auth0 ACCESS token (audience = AUTH0_AUDIENCE),
    // not the ID token. getAccessToken() returns the access token configured
    // with the API audience in lib/auth0.ts.
    const { token } = await auth0.getAccessToken();

    const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/users/me`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
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
