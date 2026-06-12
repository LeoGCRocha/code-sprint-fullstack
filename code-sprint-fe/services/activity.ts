import { auth0 } from "@/lib/auth0";

export type SubmissionActivity = Record<string, number>;

export async function getSubmissionActivity(): Promise<SubmissionActivity> {
  if (process.env.USE_MOCKS === "true") return {};

  const { token } = await auth0.getAccessToken();

  try {
    const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/users/me/submission-activity`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    });

    return res.ok ? res.json() : {};
  } catch (err) {
    console.error("[getSubmissionActivity] upstream fetch failed:", err);
    return {};
  }
}
