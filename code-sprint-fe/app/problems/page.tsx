// TODO(BE): Replace all mock data with real API calls:
// - ProblemList: fetch paginated problems from GET /problems with filter/sort params
// - ProblemFilter: filter options (difficulty, tags, status) driven by BE response
// - ProgressSidebar: fetch authenticated user progress from GET /users/me/progress
// - LiveCompetition: fetch active competition from GET /competitions/active
import { ProblemFilter } from "@/features/problems/components/ProblemFilter";
import { ProblemList } from "@/features/problems/components/ProblemList";
import { ProgressSidebar } from "@/features/problems/components/ProgressSidebar";
import { LiveCompetition } from "@/features/problems/components/LiveCompetition";

export default function ProblemsPage() {
  return (
    <div className="min-h-screen bg-background px-6 py-8">
      <div className="mx-auto grid max-w-7xl grid-cols-1 gap-8 md:grid-cols-[1fr_300px]">
        <main>
          <h1 className="text-4xl font-black">Problem Library</h1>
          <p className="mb-6 text-neutral-500">
            Sharpen your skills across various algorithmic domains.
          </p>
          <ProgressSidebar />
          <ProblemFilter />
          <ProblemList />
        </main>
        <aside className="flex flex-col gap-4">
          <LiveCompetition />
        </aside>
      </div>
    </div>
  );
}
