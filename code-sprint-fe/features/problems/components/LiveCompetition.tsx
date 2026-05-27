import { Button } from "@/components/ui/Button";

export function LiveCompetition() {
  return (
    <div className="rounded-2xl bg-neutral-900 p-5 text-white">
      <h3 className="mb-3 text-lg font-black">Live Competitions</h3>
      <div className="flex flex-col gap-3">
        <div>
          <div className="flex items-center justify-between">
            <span className="font-semibold">Weekly Sprint #45</span>
            <span className="rounded-full bg-red-500 px-2 py-0.5 text-xs font-bold">
              LIVE
            </span>
          </div>
          <p className="mt-1 text-sm text-neutral-400">
            Solve 5 algorithmic challenges in 60 minutes
          </p>
          <p className="mt-1 text-sm font-semibold text-orange-400">
            Ends in 2:34:15
          </p>
          <div className="mt-2 h-1.5 w-full rounded-full bg-neutral-700">
            <div className="h-full w-2/3 rounded-full bg-orange-400" />
          </div>
        </div>
        <Button className="w-full bg-white text-black hover:bg-neutral-100">
          Join Lobby
        </Button>
      </div>
    </div>
  );
}
