export function CurrentCompetition() {
  return (
    <div className="rounded-2xl bg-neutral-950 p-4">
      <span className="bg-primary-600 inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-xs font-bold text-white">
        <span className="h-1.5 w-1.5 rounded-full bg-white" />
        LIVE
      </span>

      <div className="flex flex-col md:flex-row md:justify-between">
        <div>
          <h1 className="mt-3 text-2xl leading-tight font-black text-white">Weekly Sprint #142</h1>
          <p className="text-sm text-neutral-400">
            Dynamic Programming, Graphs, &amp; Advanced Structures
          </p>
        </div>

        <div className="mt-3 rounded-xl bg-neutral-900 px-4 py-3 md:mt-0 md:min-h-full md:py-6">
          <div className="mt-3 flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold tracking-widest text-neutral-500 uppercase">
                Time Remaining
              </p>
              <p className="text-primary-500 text-2xl font-black">01:24:38</p>
            </div>
            <div className="text-right">
              <p className="text-xs font-semibold tracking-widest text-neutral-500 uppercase">
                Round
              </p>
              <p className="text-xl font-black text-white">2 of 4</p>
            </div>
          </div>
          <div className="hidden md:flex">
            <span className="text-sm text-neutral-300">Started at 8:00 | Ends at 9:00 </span>
          </div>
        </div>
      </div>
    </div>
  );
}
