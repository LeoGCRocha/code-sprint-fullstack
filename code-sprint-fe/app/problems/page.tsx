import { ProblemFilter } from "@/features/problems/components/ProblemFilter";
import { UserRank } from "@/features/problems/components/UserRank";
import { ProblemList } from "@/features/problems/components/ProblemList";

export default function Problems() {
  return (
    <div className="flex flex-col">
      <ProblemFilter />
      <UserRank />
      <ProblemList />
    </div>
  );
}
