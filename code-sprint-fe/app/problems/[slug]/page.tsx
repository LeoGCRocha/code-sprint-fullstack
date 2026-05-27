import { Badge } from "@/components/ui/Badge";
import { Container } from "@/components/ui/Container";
import { StatItem } from "@/components/ui/StatItem";
import { StartSolving } from "@/features/problems/components/StartSolving";
import { TabSwitcher } from "@/features/problems/components/TabSwitcher";

// PAGE routering to each slug...
export default async function ProblemPage({ params }: { params: Promise<{ slug: string }> }) {
  return (
    <div className="flex flex-col gap-5 p-4">
      <Container>
        <div className="flex flex-row justify-between">
          <div className="flex flex-row gap-2">
            <Badge variant="yellow">Medium</Badge>
            <Badge variant="red">50 pts</Badge>
          </div>
          {/* TODO: Receive this from backend */}
          <Badge variant="green">Solved</Badge>
        </div>
        <h2 className="text-2xl leading-tight font-black">Prime Factorization Optimization</h2>
        <p>Problem ID: #12312321 | Added 2 months ago</p>
        <hr className="border-neutral-200" />
        <div className="flex flex-row justify-between">
          <StatItem value="42.1k" label="Solved" />
          <StatItem value="50" label="Points" />
        </div>
      </Container>
      <TabSwitcher />
      <StartSolving />
    </div>
  );
}
