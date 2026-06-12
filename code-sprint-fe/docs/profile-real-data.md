# Profile real data wiring

## What changed

The profile page no longer uses the mocked top statistics. `app/profile/me/page.tsx`
now fetches the authenticated user on the server, redirects unauthenticated users, then
fetches submission activity and derives the visible stat cards from those backend
payloads.

The right column remains intentionally static for now:

- `features/profile/ProblemCategories`
- `features/profile/Achievements`

## Data sources

`services/users.ts` calls the gateway endpoint `GET /api/users/me` with the Auth0
access token from the server session. This returns the profile fields used by the
header and the real `points` value exposed by the Users API contract.

`services/activity.ts` calls `GET /api/users/me/submission-activity`. The Users API
returns a map shaped like `{ "yyyy-MM-dd": count }`, backed by the heatmap read model.
The frontend uses the same response for both the heatmap and the stat cards.

Both requests use `cache: "no-store"` because this is authenticated per-user data and
the task explicitly avoided adding cache behavior.

## Stat derivation

The stat cards are built in the Server Component:

- `Submissions`: sum of all counts in the activity map.
- `Points`: `user.points` from `GET /api/users/me`.
- `Streak`: consecutive days with activity ending today.
- `Active Days`: number of activity-map days with a count above zero.

This keeps the browser free of token handling and sends only serializable props to the
client heatmap grid.

## Component changes

`features/profile/StatsBar` now receives a `stats` prop instead of owning hard-coded
mock values.

`features/profile/Heatmap` now receives the already-fetched activity map and converts it
into the trailing-day grid. That avoids issuing the same authenticated request twice.
