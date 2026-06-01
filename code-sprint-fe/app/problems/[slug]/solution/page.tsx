// TODO(BE): Wire solution page to real API:
// - Fetch problem by slug from GET /problems/:slug (replace local mock)
// - CodeSolutionPanel "Submit Solution" button should POST /submissions with { problemSlug, language, code }
// - After submit, redirect to /submissions/:id with the returned submission ID
// - time/memory limits should come from the problem data, not be hardcoded
import { Badge } from "@/components/ui/Badge";
import { Container } from "@/components/ui/Container";
import { StatItem } from "@/components/ui/StatItem";
import { CodeSolutionPanel } from "@/features/problems/components/CodeSolutionPanel";
import { problems } from "@/features/problems/data";
import { ChevronLeftIcon } from "lucide-react";
import Link from "next/link";
import { notFound } from "next/navigation";

export default async function Solution({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const problem = problems.find((p) => p.slug === slug);
  if (!problem) return notFound();

  const difficultyVariant = {
    easy: "green",
    medium: "yellow",
    hard: "red",
  } as const;

  return (
    <div className="flex h-[calc(100dvh-65px)] flex-col overflow-hidden bg-background p-4 md:flex-row md:gap-0 md:p-0">
      <aside className="shrink-0 overflow-y-auto md:w-[340px] md:border-r md:border-neutral-200 md:p-5">
        <Container className="mt-0 max-w-full">
          <div className="mb-2 flex gap-2">
            <Badge variant={difficultyVariant[problem.difficulty]}>
              {problem.difficulty.charAt(0).toUpperCase() + problem.difficulty.slice(1)}
            </Badge>
            <Badge variant="red">{problem.points} pts</Badge>
          </div>
          <h2 className="text-2xl leading-tight font-black">{problem.title}</h2>
          <p>Problem ID: #{problem.slug} | ~{problem.estimatedTime}</p>
          <hr className="border-neutral-200" />
          <Link
            href={`/problems/${problem.slug}`}
            className="my-2 flex flex-row items-center justify-center gap-1 rounded-xl bg-neutral-100 p-3 text-sm font-medium transition-colors hover:bg-neutral-200"
          >
            <ChevronLeftIcon className="h-4 w-4" /> Back to Problem Details
          </Link>
          <div className="flex flex-row justify-between">
            <StatItem value="2.0 seconds" label="Time Limit" isReversed={true} />
            <StatItem value="256 MB" label="Memory Limit" isReversed={true} />
          </div>
        </Container>
      </aside>

      <div className="mt-4 flex flex-1 flex-col overflow-hidden md:mt-0 md:p-4">
        <CodeSolutionPanel fillHeight />
      </div>
    </div>
  );
}
