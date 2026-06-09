import { http, HttpResponse, delay } from "msw";
import { mockProblems } from "./data/problems";
import { mockSubmissionAck, mockSubmissionResponse } from "./data/submission";

export const handlers = [
  // Regex ignores host/port — matches the gateway path `${NEXT_PUBLIC_API_URL}/api/problems`
  http.get(/\/api\/problems$/, async ({ request }) => {
    await delay(400);

    const url = new URL(request.url);
    const difficulty = url.searchParams.get("difficulty");
    const tag = url.searchParams.get("tag");
    const page = Number(url.searchParams.get("page") ?? "1");
    const pageSize = Number(url.searchParams.get("pageSize") ?? "10");

    let results = mockProblems;

    if (difficulty && difficulty !== "all") {
      results = results.filter((p) => p.difficulty === difficulty);
    }
    if (tag) {
      results = results.filter((p) => p.tags.includes(tag));
    }

    const total = results.length;
    const items = results.slice((page - 1) * pageSize, page * pageSize);

    return HttpResponse.json({ items, total, page, pageSize });
  }),

  http.get(/\/api\/problems\/([^/]+)$/, async ({ request }) => {
    await delay(200);

    const slug = new URL(request.url).pathname.split("/").pop();
    const problem = mockProblems.find((p) => p.slug === slug);

    if (!problem) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json(problem);
  }),

  http.post(/\/api\/submissions$/, async () => {
    await delay(600);
    return HttpResponse.json(mockSubmissionAck, { status: 202 });
  }),

  http.get(/\/api\/submissions\/([^/]+)$/, async () => {
    await delay(200);
    return HttpResponse.json(mockSubmissionResponse);
  }),
];
