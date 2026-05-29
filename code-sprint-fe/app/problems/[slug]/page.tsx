import { Badge } from "@/components/ui/Badge";
import { Container } from "@/components/ui/Container";
import { StatItem } from "@/components/ui/StatItem";
import { problems } from "@/features/problems/data";
import { StartSolving } from "@/features/problems/components/StartSolving";
import { TabSwitcher } from "@/features/problems/components/TabSwitcher";
import { notFound } from "next/navigation";

export default async function ProblemPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const problem = problems.find((p) => p.id === slug);

  if (!problem) {
    notFound();
  }

  const difficultyVariant = {
    easy: "green",
    medium: "yellow",
    hard: "red",
  } as const;

  return (
    <div className="flex flex-col gap-5 p-4">
      <Container>
        <div className="flex flex-row justify-between">
          <div className="flex flex-row gap-2">
            <Badge variant={difficultyVariant[problem.difficulty]}>
              {problem.difficulty.charAt(0).toUpperCase() + problem.difficulty.slice(1)}
            </Badge>
            <Badge variant="red">{problem.points} pts</Badge>
          </div>
          <Badge variant={problem.status === "review" ? "green" : "yellow"}>
            {problem.status === "review" ? "Solved" : problem.status === "continue" ? "In Progress" : "Not Started"}
          </Badge>
        </div>
        <h2 className="text-2xl leading-tight font-black">{problem.title}</h2>
        <p>Problem ID: #{problem.id} | ~{problem.estimatedTime}</p>
        <hr className="border-neutral-200" />
        <div className="flex flex-row justify-between">
          <StatItem value={problem.solvedCount.toLocaleString()} label="Solved" />
          <StatItem value={String(problem.points)} label="Points" />
        </div>
      </Container>
      <TabSwitcher problem={problem} />
      <StartSolving />
    </div>
  );
}
