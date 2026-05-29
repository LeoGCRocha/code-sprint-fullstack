import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { Container } from "@/components/ui/Container";
import { ChevronDownIcon } from "lucide-react";

interface StartSolvingProps {
  slug: string;
}

export function StartSolving({ slug }: StartSolvingProps) {
  return (
    <Container variant="dark" className="mt-0 max-w-full gap-4">
      <h1 className="text-2xl leading-tight font-black text-white">Ready to solve?</h1>
      <small className="text-sm text-neutral-400">
        Choose your preferred language and start coding.
      </small>

      <div className="relative">
        <select className="w-full cursor-pointer appearance-none rounded-lg bg-neutral-900 px-4 py-2 text-white">
          <option value="python3">Python 3</option>
          <option value="javascript">JavaScript</option>
          <option value="cpp">C++</option>
        </select>
        <ChevronDownIcon className="pointer-events-none absolute top-1/2 right-3 h-4 w-4 -translate-y-1/2 text-white" />
      </div>

      <Link href={`/problems/${slug}/solution`} className="w-full">
        <Button className="w-full">{"</> Start Coding"}</Button>
      </Link>
    </Container>
  );
}
