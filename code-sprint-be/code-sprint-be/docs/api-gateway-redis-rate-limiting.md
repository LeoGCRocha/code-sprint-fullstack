# Distributed Rate Limiting with Redis (Multi-Replica Gateway)

Companion to [`api-gateway-yarp.md`](./api-gateway-yarp.md). That guide uses the **in-memory**
token-bucket limiter, which is correct for a **single** gateway instance. This document explains
why that breaks the moment you run **more than one** gateway replica, and how a **Redis backplane**
fixes it.

> **Read this when:** you are about to scale the gateway past one instance (k8s replicas, multiple
> Aspire instances, autoscaling). Until then, the built-in limiter is fine — don't add Redis you
> don't need.

---

## Table of contents

1. [The problem: limits multiply per replica](#1-the-problem)
2. [Why in-memory can't work across replicas](#2-why-in-memory-cant-work)
3. [The fix: a shared Redis backplane](#3-the-fix)
4. [Step 1 — Add the package](#step-1--add-the-package)
5. [Step 2 — Provision Redis in Aspire](#step-2--provision-redis-in-aspire)
6. [Step 3 — Per-user token bucket, Redis-backed](#step-3--per-user-token-bucket-redis-backed)
7. [Step 4 — Standard 429 headers (Retry-After)](#step-4--standard-429-headers)
8. [Step 5 — Decide fail-open vs fail-closed](#step-5--fail-open-vs-fail-closed)
9. [Step 6 — Verify across replicas](#step-6--verify-across-replicas)
10. [Reference — in-memory vs Redis](#reference--in-memory-vs-redis)

---

## 1. The problem

The in-memory limiter keeps its bucket counters **in the process's RAM**. Each gateway replica is
a separate process with its **own** counters. They never talk to each other.

Policy: *"20 requests/second per user."* Scale the gateway to 3 replicas behind a load balancer:

```
                          ┌──────────────┐
                          │ Load Balancer│  round-robin
                          └──┬───┬────┬───┘
              ┌──────────────┘   │    └──────────────┐
        ┌─────▼─────┐      ┌─────▼─────┐       ┌─────▼─────┐
        │ gateway-1 │      │ gateway-2 │       │ gateway-3 │
        │ bucket:   │      │ bucket:   │       │ bucket:   │
        │ user42→20 │      │ user42→20 │       │ user42→20 │   ← 3 independent buckets
        └───────────┘      └───────────┘       └───────────┘
                 user42's effective limit = 20 × 3 = 60 req/s
```

**Each replica independently grants the full 20/s.** The user's real ceiling becomes
`limit × replica_count`. Worse, the load balancer spreads one user's requests across replicas, so
each replica sees only a fraction and **nobody hits the limit** — the throttle silently
disappears. This is the "spike between replicas" you want to avoid.

---

## 2. Why in-memory can't work

A rate limit is **shared state**: "how many has *this user* spent, *everywhere*, in this window."
In-memory state is per-process by definition. No amount of tuning fixes it — three processes
cannot agree on a count they each hold privately.

The only correct fix is to move the counter **out** of any single process into a store all
replicas read and mutate **atomically**. That store is Redis.

---

## 3. The fix

Use `cristipufu/aspnetcore-redis-rate-limiting`. It keeps the **same four algorithms** (fixed
window, sliding window, **token bucket**, concurrency) but stores counters in Redis and mutates
them with **atomic Lua scripts**.

```
        ┌───────────┐   ┌───────────┐   ┌───────────┐
        │ gateway-1 │   │ gateway-2 │   │ gateway-3 │
        └─────┬─────┘   └─────┬─────┘   └─────┬─────┘
              │               │               │
              └───────────────┼───────────────┘
                              ▼
                       ┌─────────────┐
                       │    Redis    │  one bucket per user, shared
                       │ user42 → 20 │  atomic Lua = thread-safe across all replicas
                       └─────────────┘
                  user42's effective limit = 20 req/s, period.
```

**Why Lua, not a distributed lock:** the check-and-decrement runs as a single atomic script
*inside* Redis. No replica can interleave with another, and there's no lock to acquire/release —
so it's correct *and* fast, even under contention.

---

## Step 1 — Add the package

In the gateway project:

```bash
cd code-sprint-be/code-sprint-be/src/gateway/CodeSprint.Gateway
dotnet add package RedisRateLimiting
dotnet add package RedisRateLimiting.AspNetCore   # OnRejected header helper
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis  # Aspire Redis client wiring
```

- `RedisRateLimiting` — the limiter algorithms + `RedisRateLimitPartition`.
- `RedisRateLimiting.AspNetCore` — `RateLimitMetadata.OnRejected` (writes standard 429 headers).
- The Aspire Redis client component supplies the `IConnectionMultiplexer`.

---

## Step 2 — Provision Redis in Aspire

`src/aspire/CodeSprint.AppHost/AppHost.cs` — add a Redis resource and reference it from the gateway:

```csharp
var redis = builder.AddRedis("redis");   // container in dev; managed cache in prod

// ... existing postgres / services ...

builder.AddProject<Projects.CodeSprint_Gateway>("gateway")
       .WithReference(usersApi)
       .WithReference(problemsApi)
       .WithReference(redis)          // inject the "redis" connection string
       .WaitFor(redis)
       .WaitFor(usersApi)
       .WaitFor(problemsApi)
       .WithExternalHttpEndpoints();
```

Then register the Redis client in the gateway's `Program.cs` (Aspire component reads the injected
connection string named `redis`):

```csharp
builder.AddRedisClient("redis");   // registers IConnectionMultiplexer in DI
```

> One Redis instance backs the rate limiter. The same Redis can later serve YARP output caching or
> session state — but keep logical concerns on separate key prefixes.

---

## Step 3 — Per-user token bucket, Redis-backed

Important subtlety: the simple `AddRedisTokenBucketLimiter("name", ...)` registration creates **one
global bucket** for the whole policy — every user shares it. To keep the **per-user** behavior from
the main guide, partition by the `sub` claim with a small custom policy using
`RedisRateLimitPartition.GetTokenBucketRateLimiter`.

`src/gateway/CodeSprint.Gateway/RateLimiting/PerUserRedisTokenBucketPolicy.cs`:

```csharp
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using RedisRateLimiting;
using StackExchange.Redis;

namespace CodeSprint.Gateway.RateLimiting;

// Per-user token bucket whose counters live in Redis, so the limit holds across all replicas.
public sealed class PerUserRedisTokenBucketPolicy(IConnectionMultiplexer redis)
    : IRateLimiterPolicy<string>
{
    // Standard 429 headers (Retry-After etc.) — see Step 4.
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected =>
        (context, ct) =>
            RedisRateLimiting.AspNetCore.RateLimitMetadata
                .OnRejected(context.HttpContext, context.Lease, ct);

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        // Same partition key strategy as the in-memory version:
        // per authenticated user, falling back to IP, then a shared anonymous bucket.
        var partitionKey = httpContext.User.FindFirst("sub")?.Value
                           ?? httpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "anonymous";

        return RedisRateLimitPartition.GetTokenBucketRateLimiter(
            partitionKey,
            _ => new RedisTokenBucketRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => redis,
                TokenLimit          = 100,                     // burst
                TokensPerPeriod     = 20,                      // sustained refill...
                ReplenishmentPeriod = TimeSpan.FromSeconds(1)  // ...per second → 20 req/s
            });
    }
}
```

Register it under the **same policy name** the routes already reference (`"perUser"` in
`appsettings.json`), so no route config changes:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy<string, PerUserRedisTokenBucketPolicy>("perUser");
});
```

**This is a drop-in swap.** The route's `"RateLimiterPolicy": "perUser"` is unchanged; only the
implementation behind the name moved from in-memory to Redis. The middleware order from the main
guide still applies — `UseAuthentication` before `UseRateLimiter` so `sub` is populated.

---

## Step 4 — Standard 429 headers

`RateLimitMetadata.OnRejected` (wired in the policy above) emits the headers clients expect, so the
frontend can back off intelligently instead of hammering:

```
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 3
Retry-After: 3
```

`Retry-After` is the key one — a well-behaved client waits that many seconds before retrying. Have
the frontend's HTTP layer honor it.

---

## Step 5 — Fail-open vs fail-closed

New dependency = new failure mode: **what happens to traffic if Redis is unreachable?**

| Mode | Behavior when Redis is down | Risk |
|---|---|---|
| **Fail-open** | Allow the request (skip the limit) | No throttle during the outage — abuse/overload possible |
| **Fail-closed** | Reject the request (429/503) | Redis outage takes the whole gateway down |

The library throws on connection failure; wrap the partition decision so **you** choose. For a
gateway, **fail-open is usually right** — a rate limiter outage should not become a full service
outage. Pair it with:

- A circuit breaker / short Redis timeout so you don't block on a dead connection.
- An alert on Redis health (this is a degraded state, not normal).
- Optionally, fall back to the **in-memory** limiter during the outage — imperfect across replicas,
  but better than no limit at all.

Decide deliberately and document it. Silent fail-closed on a Redis blip = self-inflicted outage.

---

## Step 6 — Verify across replicas

The whole point is multi-instance correctness — so test with more than one instance.

```bash
# Run the gateway with 2+ replicas (k8s, or two local instances behind a proxy).
# Hammer a single user's token past the sustained rate:
for i in $(seq 1 150); do
  curl -s -o /dev/null -w "%{http_code}\n" \
    http://<gw>/api/problems \
    -H "Authorization: Bearer <jwt-for-user42>"
done | sort | uniq -c
```

**In-memory (broken):** with 2 replicas you'd see ~all `200` well past the limit — each replica
granted its own budget.

**Redis (correct):** the first ~100 (`TokenLimit`) succeed, then you see `429`s as the **shared**
bucket drains, regardless of which replica served each request. Confirm `Retry-After` is present on
the 429s.

---

## Reference — in-memory vs Redis

| | In-memory (`System.Threading.RateLimiting`) | Redis (`cristipufu/aspnetcore-redis-rate-limiting`) |
|---|---|---|
| Counter location | process RAM | Redis (shared) |
| Correct with 1 replica | ✅ | ✅ |
| Correct with N replicas | ❌ limit × N | ✅ single shared limit |
| External dependency | none | Redis |
| Latency per check | nanoseconds | one Redis round-trip (sub-ms, atomic Lua) |
| Algorithms | fixed/sliding/token bucket/concurrency | same four |
| New failure mode | none | Redis down → fail-open/closed decision |
| Use when | dev, single instance | 2+ gateway replicas |

**Migration is a swap, not a rewrite:** same `"perUser"` policy name, same token-bucket knobs, same
per-`sub` partition. Only the counter's home changes — RAM → Redis — which is exactly what makes the
limit hold across replicas and kills the cross-replica spike.

---

## Decision rule (one line)

> Gateway replica count `== 1` → in-memory token bucket. Replica count `> 1` → Redis-backed token
> bucket. Same policy, same knobs, counters just move to Redis.
