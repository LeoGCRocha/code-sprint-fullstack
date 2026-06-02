import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Container } from "@/components/ui/Container";

export function Profile() {
  return (
    // TODO: Load from the backend
    <Container className="mt-0 max-w-full">
      <div className="flex flex-col gap-3">
        <div className="flex flex-row gap-3">
          <div className="bg-primary-600 flex h-16 w-16 shrink-0 items-center justify-center rounded-full">
            <span className="text-2xl font-semibold text-white">JD</span>
          </div>
          <div>
            <h1 className="leading-tight font-black">John Doe</h1>
            <span className="text-sm text-neutral-500">@johndoe</span>
            <div className="mt-1.5 flex flex-row flex-wrap gap-1">
              <Badge variant="green">Expert</Badge>
              <Badge variant="yellow">Top 1%</Badge>
            </div>
          </div>
        </div>

        <p className="text-sm text-neutral-600">
          Full-stack developer & competitive programmer · CS @ MIT
        </p>

        <div className="flex items-center gap-2">
          <span className="text-xs text-neutral-400">Member since Mar 2023</span>
          <div className="flex gap-1.5">
            <Button variant="outline" size="sm">
              GitHub
            </Button>
            <Button variant="outline" size="sm">
              Portfolio
            </Button>
          </div>
        </div>
      </div>
    </Container>
  );
}
