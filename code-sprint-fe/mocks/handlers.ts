import { http, HttpResponse, delay } from "msw";
import { mockProblems } from "./data/problems";

export const handlers = [
  // Regex ignores host/port — works regardless of NEXT_PUBLIC_PROBLEMS_API_URL value
  http.get(/\/problems$/, async ({ request }) => {
    await delay(400);

    const url = new URL(request.url);
    const difficulty = url.searchParams.get("difficulty");
    const tag = url.searchParams.get("tag");

    let results = mockProblems;

    if (difficulty && difficulty !== "all") {
      results = results.filter((p) => p.difficulty === difficulty);
    }
    if (tag) {
      results = results.filter((p) => p.tags.includes(tag));
    }

    return HttpResponse.json(results);
  }),

  http.get(/\/problems\/([^/]+)$/, async ({ request }) => {
    await delay(200);

    const slug = new URL(request.url).pathname.split("/").pop();
    const problem = mockProblems.find((p) => p.slug === slug);

    if (!problem) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json(problem);
  }),
];
