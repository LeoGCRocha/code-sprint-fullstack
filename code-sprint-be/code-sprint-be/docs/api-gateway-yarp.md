# Building a Production API Gateway with YARP (.NET)

A complete, teaching-oriented walkthrough for adding a **YARP reverse-proxy gateway** to the
CodeSprint backend. The gateway becomes the single front door: it owns **routing**,
**authentication enforcement**, **CORS**, and **rate limiting**, while each domain service
stays small and internal.

> **What you'll have at the end:** one public `gateway` service. The browser talks only to it.
> `users-api` and `problems-api` stop facing the internet and are reached only through the gateway,
> resolved by name via Aspire service discovery.

---

## Table of contents

1. [Mental model: what a gateway is and isn't](#1-mental-model)
2. [Where auth happens (Pattern A) and why](#2-where-auth-happens)
3. [Prerequisite: centralized auth in ServiceDefaults](#3-prerequisite-centralized-auth)
4. [Step 1 — Create the gateway project](#step-1--create-the-gateway-project)
5. [Step 2 — Understand the four moving parts of YARP](#step-2--the-four-moving-parts-of-yarp)
6. [Step 3 — Write `Program.cs`](#step-3--write-programcs)
7. [Step 4 — Define routes and clusters in config](#step-4--routes-and-clusters)
8. [Step 5 — Wire the gateway into Aspire](#step-5--wire-into-aspire)
9. [Step 6 — Remove CORS from downstream services](#step-6--slim-downstream-services)
10. [Step 7 — Run and verify](#step-7--run-and-verify)
11. [Step 8 — Production hardening](#step-8--production-hardening)
12. [Appendix — Request lifecycle, end to end](#appendix--request-lifecycle)

---

## 1. Mental model

A **reverse proxy** sits in front of your services and forwards each incoming request to the
right one. **YARP** (Yet Another Reverse Proxy) is Microsoft's library for building one as a
normal ASP.NET app — so you get the full middleware pipeline (auth, CORS, rate limiting,
logging) *before* the request is forwarded.

```
                         ┌─────────────────────────────┐
   Browser  ──HTTPS──▶   │          GATEWAY            │
   (one URL)             │  CORS → RateLimit → Auth →  │
                         │  Authorize → Route → Proxy  │
                         └───────┬───────────┬─────────┘
                                 │           │
                  http://users-api      http://problems-api
                                 │           │
                          ┌──────▼───┐  ┌────▼──────┐
                          │ users-api│  │problems-api│   (internal only)
                          └──────────┘  └───────────┘
```

**Gateway responsibilities:** routing, auth enforcement, CORS, rate limiting, single TLS entry.
**Gateway non-responsibilities:** business logic, database access, domain rules. It only routes.

---

## 2. Where auth happens

The gateway validates the Auth0 JWT and enforces per-route authorization. The question is what
the gateway sends downstream. We use **Pattern A: forward the original token, services validate
again.**

```
Browser ─[Bearer JWT]─▶ Gateway (validate ✓, authorize ✓) ─[same Bearer JWT]─▶ users-api (validate ✓)
```

**Why Pattern A for CodeSprint:**

- The Aspire dev environment is **not network-isolated** — services are reachable directly on
  localhost. Any approach that makes services trust unauthenticated headers would be spoofable.
- Each service already validates JWTs. Keeping that = **zero-trust / defense-in-depth**: a service
  is safe even if reached directly.
- The cost (validating the token twice) is microseconds — a signature check against a cached JWKS.

The alternative (Pattern B — gateway strips the token and injects trusted `X-User-*` headers,
services skip validation) is only safe once services are network-isolated (private VPC / mTLS).
Revisit it then; you may never need it.

---

## 3. Prerequisite: centralized auth

> **Status in this repo: already done.** The auth setup that used to be copy-pasted in each
> service now lives once in `CodeSprint.ServiceDefaults`. This section explains it so the gateway
> step makes sense.

`src/aspire/CodeSprint.ServiceDefaults/AuthenticationExtensions.cs`:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

// Centralized Auth0 JWT bearer authentication for every CodeSprint service.
// Validation still runs in each service (zero-trust), but the setup lives in one place.
public static class AuthenticationExtensions
{
    public static TBuilder AddCodeSprintAuth<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority        = builder.Configuration["Auth0:Authority"];
            options.Audience         = builder.Configuration["Auth0:Audience"];
            options.MapInboundClaims = false;
        });

        builder.Services.AddAuthorization();
        return builder;
    }
}
```

Every service `Program.cs` then reduces to one line:

```csharp
builder.AddCodeSprintAuth();
```

**Key idea:** the *runtime check* repeats per service on purpose (zero-trust). The *setup code*
does not — it lives in one place. Add a new domain → one line → it inherits the same auth.

The gateway will call the **same** `AddCodeSprintAuth()`, so the front door and the services share
identical token-validation rules.

---

## Step 1 — Create the gateway project

```bash
cd code-sprint-be/code-sprint-be/src
mkdir gateway && cd gateway
dotnet new web -n CodeSprint.Gateway
cd CodeSprint.Gateway

# YARP itself
dotnet add package Yarp.ReverseProxy

# Bridges YARP destinations (http://users-api) to Aspire service discovery
dotnet add package Microsoft.Extensions.ServiceDiscovery.Yarp

# Shared platform setup: telemetry, health, resilience, AND AddCodeSprintAuth()
dotnet add reference ../../aspire/CodeSprint.ServiceDefaults/CodeSprint.ServiceDefaults.csproj

# Register in the solution
dotnet sln ../../code-sprint-be.sln add CodeSprint.Gateway/CodeSprint.Gateway.csproj
```

Why each package:

| Package | Purpose |
|---|---|
| `Yarp.ReverseProxy` | The proxy engine — routes, clusters, transforms |
| `Microsoft.Extensions.ServiceDiscovery.Yarp` | Lets a destination address be a logical name (`http://users-api`) instead of a hardcoded host:port |
| `CodeSprint.ServiceDefaults` (project ref) | Gives the gateway `AddServiceDefaults()` and `AddCodeSprintAuth()` — JwtBearer comes transitively, no extra package needed |

---

## Step 2 — The four moving parts of YARP

Before code, learn the vocabulary. Everything in YARP is one of these:

| Concept | What it is | Analogy |
|---|---|---|
| **Route** | A match rule (path, method, headers) + which cluster to send to + which auth/rate-limit policy applies | "GET /api/problems/* → problems cluster, public" |
| **Cluster** | A named group of one or more destinations + load-balancing/health settings | "the problems service" |
| **Destination** | An actual address to forward to | `http://problems-api` |
| **Transform** | A mutation applied to the request/response as it passes through (rewrite path, add/strip headers) | "drop the `/api/problems` prefix" |

A request flows: **match a Route → pick its Cluster → choose a Destination → apply Transforms → forward.**

---

## Step 3 — Write `Program.cs`

`src/gateway/CodeSprint.Gateway/Program.cs`:

```csharp
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// (1) Aspire platform defaults: telemetry, health checks, service discovery, HTTP resilience.
builder.AddServiceDefaults();

// (2) Same Auth0 JWT validation the services use — the gateway is now the enforcement point.
builder.AddCodeSprintAuth();

// (3) Authorization policies that routes will reference by name (see Step 4).
builder.Services.AddAuthorization(options =>
{
    // Any authenticated caller.
    options.AddPolicy("authenticated", p => p.RequireAuthenticatedUser());

    // Example scoped policy — Auth0 puts granted permissions in the "permissions" claim.
    options.AddPolicy("problems:write", p =>
        p.RequireAuthenticatedUser()
         .RequireClaim("permissions", "write:problems"));

    // Deny-by-default: a route with no explicit policy still requires auth.
    // Public routes must opt out with "AuthorizationPolicy": "Anonymous".
    options.FallbackPolicy = options.GetPolicy("authenticated");
});

// (4) CORS lives here now, not in each service. The browser only ever talks to the gateway.
const string frontendCors = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCors, policy =>
        policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// (5) Rate limiting at the edge — token bucket, partitioned per user (IP fallback for anon).
//     Token bucket allows short human bursts while capping the sustained rate. See "Why token
//     bucket" below. NOTE: this runs AFTER UseAuthentication so the "sub" claim is populated.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("perUser", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.User.FindFirst("sub")?.Value          // per authenticated user
                          ?? httpContext.Connection.RemoteIpAddress?.ToString() // anon → per IP
                          ?? "anonymous",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit          = 100,                      // bucket size = max burst
                TokensPerPeriod     = 20,                       // refill amount...
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),  // ...per second → 20 req/s sustained
                AutoReplenishment   = true,
                QueueLimit          = 0                         // over budget → 429 immediately, no wait
            }));
});

// (6) YARP: load routes/clusters from config + resolve destinations via Aspire.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(context =>
    {
        // Defense: never let a client spoof trusted identity headers through the gateway.
        context.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Remove("X-User-Sub");
            return default;
        });
        // Pattern A forwards the original Authorization header untouched — nothing else needed.
    });

var app = builder.Build();

// Middleware ORDER MATTERS — see note below.
app.UseCors(frontendCors);
app.UseAuthentication();   // establish WHO the caller is first...
app.UseRateLimiter();      // ...so the token-bucket policy can partition by their "sub" claim
app.UseAuthorization();

app.MapDefaultEndpoints();   // /health and /alive from ServiceDefaults
app.MapReverseProxy();       // the proxy itself; per-route auth/rate-limit enforced here

app.Run();
```

### Why token bucket is the right algorithm here

ASP.NET Core offers four limiter algorithms: fixed window, sliding window, **token bucket**, and
concurrency. Token bucket fits CodeSprint best because it separates two things every other
algorithm conflates: **how big a burst you tolerate** vs **what sustained rate you allow**.

How it works: the bucket holds `TokenLimit` tokens (100). Each request spends one. Every
`ReplenishmentPeriod` (1s) the bucket regains `TokensPerPeriod` tokens (20), never exceeding the
cap. So:

- **Idle users bank capacity.** A user who paused now has a full bucket and can fire ~100 requests
  back-to-back — then settles to 20/s.
- **Sustained rate is hard-capped** at the refill rate (20 req/s), no matter how long they hammer.

Why that matches this app:

| Need | Token bucket behavior |
|---|---|
| **Human usage is bursty** — open a problem, the page fans out to several API calls at once | Burst (`TokenLimit`) absorbs the fan-out without false 429s |
| **But no one should sustain a flood** | Refill rate caps long-run throughput |
| **Fairness per user** | Combined with per-`sub` partitioning, each user gets their own bucket |

Contrast with the alternatives:

- **Fixed window** (what this doc used before): a counter that resets every N seconds. Simple, but
  a client can send a full window's worth at `t=9.9s` and another full window at `t=10.1s` — **2×
  the limit across the boundary**. And it can't express "allow a burst but limit the average."
- **Sliding window**: fixes the boundary burst, but still ties burst size to the window length —
  you can't allow a big burst *and* a low average independently.
- **Concurrency**: limits simultaneous in-flight requests, not rate over time — a different control
  (good for protecting a scarce downstream, not for per-user fairness).

Token bucket gives the one behavior a user-facing API gateway actually wants: **tolerate the
natural burst, cap the sustained abuse.** Tune the two knobs independently — raise `TokenLimit`
for burstier UIs, lower `TokensPerPeriod` to tighten the long-run ceiling.

### Why the middleware order is non-negotiable

```
UseCors → UseAuthentication → UseRateLimiter → UseAuthorization → MapReverseProxy
```

- **CORS first** so browser preflight (`OPTIONS`) is answered before anything rejects it.
- **Authentication before RateLimiter** — the rate-limit policy partitions by the user's `sub`
  claim. That claim only exists after `UseAuthentication` populates `HttpContext.User`. Put the
  limiter first and `sub` is empty, so *every* authenticated user collapses into the IP/`anonymous`
  bucket — user A's traffic then throttles user B. Auth first = correct per-user buckets.
- **Authentication before Authorization** — you must know *who* the caller is before deciding
  *whether* they're allowed. `UseAuthorization` reads the `HttpContext.User` that authentication set.
- **MapReverseProxy last** — by the time the request reaches the proxy, identity is established
  and the route's `AuthorizationPolicy` can be enforced.

Get this wrong and you either leak unauthenticated traffic downstream, break CORS preflight, or
silently merge every user into one rate-limit bucket.

> **DoS nuance:** rate-limiting *after* auth means you pay JWT validation even for requests you'll
> reject. For raw flood protection, add a second, cheap IP-keyed limiter *before* `UseAuthentication`
> as an outer layer, keeping this per-user token bucket as the inner one.

---

## Step 4 — Routes and clusters

`src/gateway/CodeSprint.Gateway/appsettings.json`:

```jsonc
{
  "Auth0": {
    "Authority": "https://YOUR_TENANT.auth0.com/",
    "Audience": "https://api.codesprint"
  },
  "Cors": { "FrontendOrigin": "http://localhost:3000" },

  "ReverseProxy": {
    "Routes": {
      "users-route": {
        "ClusterId": "users-cluster",
        "AuthorizationPolicy": "authenticated",
        "RateLimiterPolicy": "perUser",
        "Match": { "Path": "/api/users/{**catch-all}" },
        "Transforms": [ { "PathPattern": "/{**catch-all}" } ]
      },

      "problems-read-route": {
        "ClusterId": "problems-cluster",
        "AuthorizationPolicy": "Anonymous",
        "Match": { "Path": "/api/problems/{**catch-all}", "Methods": [ "GET" ] },
        "Transforms": [ { "PathPattern": "/{**catch-all}" } ]
      },

      "problems-write-route": {
        "ClusterId": "problems-cluster",
        "AuthorizationPolicy": "problems:write",
        "RateLimiterPolicy": "perUser",
        "Match": { "Path": "/api/problems/{**catch-all}", "Methods": [ "POST", "PUT", "DELETE" ] },
        "Transforms": [ { "PathPattern": "/{**catch-all}" } ]
      }
    },

    "Clusters": {
      "users-cluster": {
        "Destinations": {
          "primary": { "Address": "http://users-api" }
        }
      },
      "problems-cluster": {
        "Destinations": {
          "primary": { "Address": "http://problems-api" }
        }
      }
    }
  }
}
```

### Reading this config

- **`Address: http://users-api`** — `users-api` is the *logical* name you gave the project in
  Aspire (`AppHost.cs`). The service-discovery resolver swaps it for the real host:port at runtime.
  No ports hardcoded; works the same in dev and prod.

- **`PathPattern` transform** rewrites the forwarded path. The browser calls
  `/api/problems/123`; the transform strips the prefix so `problems-api` receives `/123` — exactly
  what its endpoints already expect. The frontend uses one base URL for everything.

- **`"AuthorizationPolicy": "Anonymous"`** is a built-in YARP value meaning "public" — it bypasses
  the `FallbackPolicy`. Used here so anyone can *browse* problems (GET) without a token.

- **Two problems routes, split by HTTP method.** Reading is public; creating/updating/deleting
  requires the `problems:write` permission. YARP matches the more specific route first, so the
  method-restricted routes win over a plain catch-all.

> **Per-environment config:** keep route shapes in `appsettings.json`; override only secrets
> (Auth0, origins) per environment. In production, inject `Auth0:*` from a secret store, not a file.

---

## Step 5 — Wire into Aspire

`src/aspire/CodeSprint.AppHost/AppHost.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("codesprint-pgdata")
    .WithPgAdmin();

var usersDb    = postgres.AddDatabase("usersdb");
var problemsDb = postgres.AddDatabase("problemsdb");

// Domain services — internal only now (note: no WithExternalHttpEndpoints).
var usersApi = builder.AddProject<Projects.CodeSprint_Users>("users-api")
       .WithReference(usersDb)
       .WaitFor(usersDb);

var problemsApi = builder.AddProject<Projects.CodeSprint_Problems>("problems-api")
       .WithReference(problemsDb)
       .WaitFor(problemsDb);

// Gateway — the ONLY public-facing project.
builder.AddProject<Projects.CodeSprint_Gateway>("gateway")
       .WithReference(usersApi)      // injects discovery config so http://users-api resolves
       .WithReference(problemsApi)   // ...and http://problems-api
       .WaitFor(usersApi)
       .WaitFor(problemsApi)
       .WithExternalHttpEndpoints(); // expose to the internet

builder.Build().Run();
```

**What `WithReference(usersApi)` does:** it injects configuration (e.g.
`services__users-api__http__0`) into the gateway. The `AddServiceDiscoveryDestinationResolver()`
from Step 3 reads that to turn `http://users-api` into the real address. This is the glue between
YARP and Aspire.

**`WithExternalHttpEndpoints()` only on the gateway** is what makes the services internal. After
this, the Aspire dashboard shows a public URL only for `gateway`.

---

## Step 6 — Slim downstream services

Now that the gateway owns CORS, remove it from `users-api` and `problems-api`. In each
`Program.cs`, delete the `AddCors(...)` registration and the `app.UseCors(...)` call.

> **Leave the services' JWT validation in place** — that's Pattern A (defense-in-depth). You are
> only removing CORS, which is purely a browser concern and the browser now only talks to the
> gateway.

After this, the services no longer need `Cors:FrontendOrigin` in their settings.

---

## Step 7 — Run and verify

```bash
dotnet run --project src/aspire/CodeSprint.AppHost
```

Open the Aspire dashboard. Confirm:

- `gateway` has a public URL.
- `users-api` and `problems-api` have **no** external endpoint.

Then exercise the gateway (`<gw>` = the gateway URL):

```bash
# Public GET — no token, expect 200
curl http://<gw>/api/problems

# Protected — no token, expect 401
curl -i http://<gw>/api/users/me

# Protected — with a valid Auth0 token, expect 200
curl http://<gw>/api/users/me -H "Authorization: Bearer <jwt>"

# Write without the write:problems permission — expect 403
curl -i -X POST http://<gw>/api/problems -H "Authorization: Bearer <jwt-without-scope>"
```

Verification checklist:

- [ ] Public GET works without a token.
- [ ] Protected route returns 401 without a token, 200 with one.
- [ ] Write route returns 403 for a token lacking `write:problems`.
- [ ] Path rewrite works (`/api/users/me` reaches the `users-api` `/me` endpoint).
- [ ] Services are unreachable from outside the gateway.

---

## Step 8 — Production hardening

| Concern | Action |
|---|---|
| **TLS** | Terminate HTTPS at the gateway; enable HSTS. Internal hop may stay HTTP only if the network is isolated; otherwise use mTLS. |
| **Secrets** | Auth0 settings and connection strings from a secret store (Key Vault), never `appsettings.json`. |
| **Rate limiting** | Per-user **token bucket** via the `sub` claim (Step 3). Tune `TokenLimit` (burst) and `TokensPerPeriod`/`ReplenishmentPeriod` (sustained) per route class. |
| **Distributed limits** | The built-in limiter is **in-memory, per process** — with 2+ gateway replicas each counts separately, so "20/s per user" becomes 20/s × replicas. Move counters to Redis (`cristipufu/aspnetcore-redis-rate-limiting`, atomic Lua) the moment the gateway scales past one instance. Full walkthrough: [`api-gateway-redis-rate-limiting.md`](./api-gateway-redis-rate-limiting.md). |
| **Timeouts** | Set `Cluster.HttpRequest.ActivityTimeout` so a slow service can't hang the gateway. |
| **Active health checks** | Configure cluster health checks hitting downstream `/health` (provided by `MapDefaultEndpoints`). |
| **Resilience** | `AddServiceDefaults()` adds Polly retry/circuit-breaker to the proxy's HttpClient automatically. |
| **Header hygiene** | Strip spoofed trusted headers (shown in the transform). Drop hop-by-hop headers. |
| **Observability** | OpenTelemetry traces propagate gateway → service automatically via ServiceDefaults — one trace spans the whole request. |
| **Deny-by-default** | `FallbackPolicy` requires auth; public routes must opt in with `Anonymous`. |
| **Body limits** | Cap `MaxRequestBodySize` at the gateway to blunt large-payload abuse. |

### Cluster with timeout + active health check (example)

```jsonc
"problems-cluster": {
  "HttpRequest": { "ActivityTimeout": "00:00:30" },
  "HealthCheck": {
    "Active": {
      "Enabled": true,
      "Path": "/health",
      "Interval": "00:00:10",
      "Timeout": "00:00:05"
    }
  },
  "Destinations": {
    "primary": { "Address": "http://problems-api" }
  }
}
```

---

## Appendix — Request lifecycle

Tracing `POST /api/problems` with a valid token, end to end:

```
1. Browser            POST http://<gw>/api/problems   (Authorization: Bearer JWT)
2. Gateway CORS        preflight/headers OK
3. Gateway AuthN       validate JWT against Auth0 JWKS → HttpContext.User populated
4. Gateway RateLimiter token bucket keyed on sub → has a token → spend one → continue
5. Gateway routing     match "problems-write-route" (path + POST method)
6. Gateway AuthZ       enforce "problems:write" → requires permissions=write:problems
                       ├─ missing → 403, request never leaves the gateway
                       └─ present → continue
7. Gateway transform   strip "X-User-Sub" spoof; rewrite path /api/problems → /
8. Service discovery   http://problems-api → real host:port
9. Forward             POST http://<resolved>/   (Authorization: Bearer JWT, unchanged)
10. problems-api       validates the SAME JWT again (Pattern A, zero-trust) → handles request
11. Response           flows back through the gateway to the browser
```

Notice the token is validated at **step 4** (gateway) and **step 10** (service). That repetition
is Pattern A working as designed — each tier trusts no one and checks for itself.
