import { ProblemList } from "@/components/ProblemList";
import { ProblemSelection } from "@/components/ProblemSelection";

export default function Problems() {
  return (
    <div className="flex flex-col">
      <ProblemSelection />
      <ProblemList />
    </div>
  );
}
