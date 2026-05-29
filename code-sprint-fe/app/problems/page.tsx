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
