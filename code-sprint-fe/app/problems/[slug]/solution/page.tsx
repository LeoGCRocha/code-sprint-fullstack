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

  return (
    <div className="flex flex-col p-3">
      <Container>
        <div className="mb-2 flex gap-2">
          <Badge variant="blue">Medium</Badge>
          <Badge variant="red">{problem.points} pts</Badge>
        </div>
        <h2 className="text-2xl leading-tight font-black">{problem.title}</h2>
        <p>Problem ID: #{problem.slug} | ~{problem.estimatedTime}</p>
        <hr className="border-neutral-200" />
        <Link
          href={`/problems/${problem.slug}`}
          className="my-2 flex flex-row items-center justify-center rounded-xl bg-neutral-300 p-3"
        >
          <ChevronLeftIcon /> Back to Problem Details
        </Link>
        <div className="flex flex-row justify-between">
          <StatItem value={"Time Limit"} label={"2.0 seconds"} isReversed={true} />
          <StatItem value={"Memory Limit"} label={"256MB"} isReversed={true} />
        </div>
      </Container>

      <CodeSolutionPanel />
    </div>
  );
}
