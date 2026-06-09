import { auth0 } from "@/lib/auth0";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function fowardToApi(
  path: string,
  init?: { method?: string; body?: string | null }
): Promise<Response> {
  let token: string;

  try {
    ({ token } = await auth0.getAccessToken());
  } catch {
    return Response.json({ error: "unauthenticated" }, { status: 401 });
  }

  let upstream: Response;

  try {
    upstream = await fetch(`${API_URL}${path}`, {
      method: init?.method ?? "GET",
      headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json" },
      body: init?.body ?? null,
      cache: "no-store",
    });
  } catch {
    return Response.json({ error: "gateway_unreachable" }, { status: 502 });
  }

  const text = await upstream.text();
  return new Response(text, {
    status: upstream.status,
    headers: { "Content-Type": upstream.headers.get("Content-Type") ?? "application/json" },
  });
}
