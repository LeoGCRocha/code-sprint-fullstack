import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Container } from "@/components/ui/Container";
import { StatItem } from "@/components/ui/StatItem";
import { problems } from "@/features/problems/data";
import { ChevronLeftIcon, SendIcon } from "lucide-react";
import { notFound } from "next/navigation";

export default async function Solution({ params }: { params: Promise<{ slug: string }> }) {
    const { slug } = await params;

    const problem = problems.find((p) => p.slug === slug);

    if (!problem) return notFound();

    return <div className="flex flex-col p-3">
        <Container>
            <div className="flex gap-2 mb-2">
                <Badge variant="blue">Medium</Badge>
                <Badge variant="red">50 points</Badge>
            </div>
            <h2 className="text-2xl leading-tight font-black">{problem.title}</h2>
            <p>Problem ID: #{problem.slug} | ~{problem.estimatedTime}</p>
            <hr className="border-neutral-200"/>
            <button className="flex flex-row justify-center items-center p-3 bg-neutral-300 rounded-xl my-2">
                <ChevronLeftIcon /> Back to Problem Details
            </button>

            <div className="flex flex-row justify-between">
                <StatItem value={"Time Limit"} label={"2.0 seconds"} isReversed={true}></StatItem>
                <StatItem value={"Memory Limit"} label={"256MB"} isReversed={true}></StatItem>
            </div>
        </Container>

        <Container className="flex flex-row justify-between max-h-fit">
            <StatItem value={"Language"} label={"C++"} isReversed={true}/>
            <div>
                <div>CHANGE</div>
            </div>
        </Container>

        <Button className="flex gap-2 p-4 mt-2 items-center justify-center">
            <SendIcon /> Submit Solution
        </Button>
    </div>
}
