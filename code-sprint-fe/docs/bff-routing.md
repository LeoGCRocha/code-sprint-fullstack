# Frontend Routing: the Backend-for-Frontend (BFF) layer

A teaching-oriented walkthrough for how the Code Sprint frontend talks to the backend
for **authenticated** calls — starting with submissions. By the end you'll understand
*why* the browser never calls the gateway directly for protected endpoints, and how
the Next.js route handlers in `app/api/*` act as a thin authenticated proxy.

> **What you'll have:** the browser calls a same-origin route like `/api/submissions`;
> a Next route handler attaches the Auth0 access token server-side and forwards to the
> API gateway. The token never reaches client JavaScript.

> **Environment note:** this project runs **Next.js 16** and **@auth0/nextjs-auth0 v4**.
> Several conventions differ from older Next (see `AGENTS.md`). Every Next API used
> here was verified against `node_modules/next/dist/docs/`.

---

## Table of contents

1. [The problem: where does the token live?](#1-the-problem)
2. [The pattern: Backend-for-Frontend](#2-the-pattern)
3. [Request lifecycle, end to end](#3-request-lifecycle)
4. [Next 16 / auth0 v4 specifics you must know](#4-specifics)
5. [Step 1 — the forwarding helper (`lib/bff.ts`)](#step-1)
6. [Step 2 — the create route (`app/api/submissions/route.ts`)](#step-2)
7. [Step 3 — the status route (`app/api/submissions/[id]/route.ts`)](#step-3)
8. [Step 4 — the client service (`services/submissions.ts`)](#step-4)
9. [Step 5 — consuming it from the editor (polling)](#step-5)
10. [How `proxy.ts` (middleware) interacts](#6-proxy)
11. [Prerequisite on the BE side: the gateway route](#7-gateway)
12. [How to test](#8-test)
13. [Why not just call the gateway from the browser?](#9-why-not)

---

## 1. The problem

There are two kinds of calls the frontend makes:

| Call | Auth? | How it's done today |
|---|---|---|
| `GET /api/problems` (browse problems) | public | client `fetch` straight to the gateway (`services/problem.ts`) |
| `GET /api/users/me` (profile) | protected | **server-side** fetch with a bearer token (`services/users.ts`) |
| `POST /api/submissions` (submit code) | protected | **triggered by a click in the browser** — new problem |

The first is easy: no token, the browser can call the gateway directly.

The third is the hard one. Submitting code is a protected action, so it needs the Auth0
**access token**. But that token lives in an **httpOnly session cookie** — by design,
client-side JavaScript *cannot read it* (that's what protects it from XSS). So the
browser literally cannot put `Authorization: Bearer …` on the request itself.

`services/users.ts` sidesteps this because it runs on the **server** (a Server
Component) where `auth0.getAccessToken()` is available. But a submission is triggered by
a user interaction in a **client** component — there is no server context at click time.

**We need a server-side endpoint, owned by the frontend, that the client can call.**

---

## 2. The pattern

That endpoint is a **Backend-for-Frontend (BFF)**: a thin server-side proxy that lives
inside the Next app. The client calls a same-origin `/api/*` route; the route handler
(server code) reads the token from the session and forwards the request to the gateway.

```
                         same-origin                     cross-origin
   ┌─────────┐  /api/submissions   ┌──────────────┐  /api/submissions  ┌─────────┐
   │ browser │ ──────────────────▶ │ Next route   │ ─────────────────▶ │ gateway │
   │ (client)│   (cookie only)     │ handler (BFF)│  (Bearer token)    │  :5298  │
   └─────────┘                     └──────────────┘                    └────┬────┘
        ▲   no access token ever       attaches token                       │
        │   touches the browser        from the session                     ▼
        └──────────────── response passed straight back ──────────── submissions-api
```

Two routes are the same path on purpose: the client calls `/api/submissions` on the
Next origin (port 3000); the BFF forwards to `/api/submissions` on the gateway
(port 5298). Same shape, different host — the only thing the BFF adds is the token.

---

## 3. Request lifecycle

Creating a submission, click to verdict:

```
1. User clicks "Submit"            client component calls createSubmission()
2. fetch POST /api/submissions     same-origin → no token needed here
3. proxy.ts (middleware)           sees an authenticated user → passes through
4. Route handler POST()            runs on the server
5. forwardToApi()                  auth0.getAccessToken() → Bearer token
6. fetch → gateway                 POST http://localhost:5298/api/submissions
7. gateway                         validates JWT, strips /api, → submissions-api
8. submissions-api                 creates Submission (Pending), 202 { id }
9. response bubbles back           browser receives { id, status: "pending" }
10. client polls GET /api/submissions/:id   every ~1s
11. ... → BFF → gateway → submissions-api    status: running → completed
12. UI shows the verdict
```

Steps 5–7 repeat the token-attach + proxy hop for every protected call. That hop is the
entire job of the BFF layer.

---

## 4. Specifics you must know (Next 16 / auth0 v4)

- **Route Handlers** live in `app/**/route.ts` and export functions named after HTTP
  verbs (`GET`, `POST`, …). They use the Web `Request`/`Response` APIs. There **cannot**
  be a `route.ts` and a `page.tsx` at the same path.
- **`params` is a Promise.** Dynamic segments arrive as
  `{ params }: { params: Promise<{ id: string }> }` — you must `await params`. (This
  changed in Next 15; in Next 16 it's the norm.)
- **Not cached by default.** `GET` route handlers are dynamic unless you opt in
  (`export const dynamic = 'force-static'`). Our proxy must stay dynamic — good.
- **`middleware.ts` was renamed to `proxy.ts`** in Next 16. The file at the repo root
  (`proxy.ts`) IS the middleware. Don't create a `middleware.ts`.
- **auth0 v4 `getAccessToken()`** works in Route Handlers, Server Components, and Server
  Actions. In a Route Handler it can also refresh + persist an expired token. It returns
  `{ token, expiresAt, scope?, token_type?, audience? }`. The token's audience is set in
  `lib/auth0.ts` (`authorizationParameters.audience`), so it's the API access token the
  gateway expects — not the ID token.

---

## Step 1 — the forwarding helper

`lib/bff.ts` is the heart of the layer: one function every protected route reuses.

```ts
import { auth0 } from "@/lib/auth0";

const API_URL = process.env.NEXT_PUBLIC_API_URL; // the gateway, e.g. http://localhost:5298

export async function forwardToApi(
  path: string,
  init?: { method?: string; body?: string | null }
): Promise<Response> {
  // 1. token from the server-side session (401 if not logged in)
  let token: string;
  try {
    ({ token } = await auth0.getAccessToken());
  } catch {
    return Response.json({ error: "unauthenticated" }, { status: 401 });
  }

  // 2. forward to the gateway with the bearer token; never cache
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

  // 3. pass the upstream status + body straight back
  const text = await upstream.text();
  return new Response(text, {
    status: upstream.status,
    headers: { "Content-Type": upstream.headers.get("Content-Type") ?? "application/json" },
  });
}
```

Why each piece:
- **`try/catch` around `getAccessToken`** — turns "no session" into a clean `401` instead
  of a thrown error.
- **`cache: "no-store"`** — this is a live authenticated proxy; a cached response would
  leak one user's data to another or serve stale status.
- **pass-through of status + body** — the BFF is dumb on purpose. It adds auth and nothing
  else; the client sees exactly what the BE returned.

---

## Step 2 — the create route

`app/api/submissions/route.ts`:

```ts
import type { NextRequest } from "next/server";
import { forwardToApi } from "@/lib/bff";

export async function POST(request: NextRequest) {
  const body = await request.text();
  return forwardToApi("/api/submissions", { method: "POST", body });
}
```

`request.text()` reads the raw JSON body and forwards it verbatim — the BFF doesn't need
to understand the payload, just relay it.

---

## Step 3 — the status route

`app/api/submissions/[id]/route.ts`:

```ts
import type { NextRequest } from "next/server";
import { forwardToApi } from "@/lib/bff";

export async function GET(
  _request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;          // Next 16: params is a Promise
  return forwardToApi(`/api/submissions/${encodeURIComponent(id)}`);
}
```

The `[id]` folder name is the dynamic segment. `encodeURIComponent` guards against weird
ids in the path.

---

## Step 4 — the client service

`services/submissions.ts` is what components import. It calls the **same-origin** BFF
routes — never the gateway — so no token handling on the client.

```ts
export async function createSubmission(input: CreateSubmissionInput) {
  const res = await fetch("/api/submissions", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) throw new Error(`createSubmission failed: HTTP ${res.status}`);
  return res.json();
}

export async function getSubmission(id: string) {
  const res = await fetch(`/api/submissions/${id}`);
  if (!res.ok) throw new Error(`getSubmission failed: HTTP ${res.status}`);
  return res.json();
}
```

The file also declares the TypeScript types mirroring the BE contract (`Language`,
`SubmissionStatus`, `Verdict`, `Evaluation`, …). **Watch the `Language` values**: the BE
enum names are `Python | JavaScript | TypeScript | Cpp | Java | CSharp | Go` — the editor's
`<select>` currently uses display values like `python3`, which must be mapped before sending.

---

## Step 5 — consuming it from the editor (next step, not yet wired)

In the editor (client component on `app/problems/[slug]/solution/`):

```tsx
"use client";
import { useMutation, useQuery } from "@tanstack/react-query";
import { createSubmission, getSubmission } from "@/services/submissions";

// 1. submit
const submit = useMutation({ mutationFn: createSubmission });

// 2. poll until terminal
const { data } = useQuery({
  queryKey: ["submission", submit.data?.id],
  queryFn: () => getSubmission(submit.data!.id),
  enabled: !!submit.data?.id,
  refetchInterval: (q) =>
    ["completed", "failed"].includes(q.state.data?.status ?? "") ? false : 1000,
});
```

`refetchInterval` returning `false` stops polling once the submission is terminal.
(TanStack Query is already a dependency — see the project's query provider.)

---

## 6. How `proxy.ts` interacts

`proxy.ts` (the middleware) runs before route handlers. Its matcher covers `/api/*`.
For a logged-in user it calls `auth0.middleware(request)` and passes through, so the
route handler runs normally. For a request with **no session** it redirects to
`/auth/login`. Since submitting requires being logged in, that's acceptable — but be
aware an unauthenticated XHR to `/api/submissions` gets a redirect, not a JSON 401.
If you later want pure-JSON 401s for `/api/*`, exclude those paths from the redirect
branch in `proxy.ts`.

---

## 7. Prerequisite on the BE side: the gateway route

The BFF forwards to `${API_URL}/api/submissions` — so the **gateway must have a route**
for it. Add to `gateway/appsettings.Development.json`:

```jsonc
"routes": {
  "submissions-route": {
    "ClusterId": "submissions-cluster",
    "AuthorizationPolicy": "authenticated",
    "RateLimiterPolicy": "perUser",
    "Match": { "Path": "/api/submissions/{**catch-all}" },
    "Transforms": [ { "PathRemovePrefix": "/api" } ]
  }
},
"clusters": {
  "submissions-cluster": {
    "Destinations": { "primary": { "Address": "http://submissions-api" } }
  }
}
```

> **Routing caveat:** the BE also exposes `GET /users/{id}/submissions` and
> `/users/{id}/submission-activity` on the submissions-api. Through the gateway,
> `/api/users/*` matches the **users-route** and goes to users-api — so those two
> endpoints are unreachable as-is. Either move them under `/submissions/...` on the
> submissions-api, or add a more specific gateway route. Not needed for create + poll.

---

## 8. How to test

1. Docker + AppHost running (gateway, users-api, submissions-api, Postgres).
2. Add the gateway route from §7.
3. `pnpm dev` (frontend on :3000), log in via Auth0.
4. From the browser console (logged in):
   ```js
   await fetch("/api/submissions", {
     method: "POST",
     headers: { "Content-Type": "application/json" },
     body: JSON.stringify({ problemId: "<GUID>", language: "Python", sourceCode: "print(1)" }),
   }).then(r => r.json());
   // → { id, status: "pending" }
   ```
5. Poll: `await fetch("/api/submissions/<id>").then(r => r.json())` → watch it reach
   `completed` with `evaluation.verdict === "Accepted"`.

To isolate the BFF from the UI, the browser-console calls above are enough — no editor
wiring required.

---

## 9. Why not just call the gateway from the browser?

You technically *could* if you exposed the token to the client — but you shouldn't:

- **Token exposure.** Putting the access token in client JS (or a non-httpOnly cookie)
  makes it stealable via XSS. The httpOnly session cookie + BFF keeps it server-only.
- **CORS.** The browser calling `:5298` cross-origin needs CORS on the gateway; the
  same-origin `/api/*` call doesn't.
- **One seam to evolve.** Token refresh, retries, header hygiene, request shaping all
  live in `lib/bff.ts` instead of being scattered across components.

The public `GET /api/problems` skips the BFF precisely because it needs no token — there's
nothing to protect, so the browser hits the gateway directly. Protected = BFF; public =
direct. That's the rule.
```
