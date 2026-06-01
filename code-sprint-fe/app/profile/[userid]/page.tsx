import { Container } from "@/components/ui/Container";
import { Heatmap } from "@/features/profile/Heatmap";

export default function ProfilePage() {
  return (
    <div className="grid grid-cols-[1fr_500px] gap-4 p-3">
      <Heatmap />

      <Container>
        <h1>Problem Categories</h1>
      </Container>
    </div>
  );
}
