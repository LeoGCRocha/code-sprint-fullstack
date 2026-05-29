import { DumbbellIcon } from "lucide-react";
import { Container } from "@/components/ui/Container";
import { Button } from "@/components/ui/Button";
import Link from "next/link";

export function PracticeModeCard() {
  return (
    <Container variant="light" title="Pratice Mode" icon={<DumbbellIcon />}>
      <p className="text-text-secondary">
        Hone your skills at your own pace. Browse thousands of problems categorized by difficulty
        and topic. Perfect for learning and interview prep.
      </p>
      <div className="m-1 grid grid-cols-2 gap-2 p-1">
        {["Mathematical", "Logical", "Geometrics", "Strings"].map((tag) => (
          <span
            key={tag}
            className="text-text-secondary rounded-xl bg-neutral-100 px-4 py-2 text-sm"
          >
            {tag}
          </span>
        ))}
      </div>
      <Button>
        <Link href="/problems">Explore problems</Link>
      </Button>
    </Container>
  );
}
