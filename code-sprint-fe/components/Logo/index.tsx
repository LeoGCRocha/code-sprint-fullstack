import Link from "next/link";
import { TypeWriter } from "@/components/TypeWriter";

export function Logo() {
  return (
    <Link href="/">
      <div className="flex flex-row items-center justify-center gap-2">
        <div className="flex items-center justify-center rounded-xl bg-neutral-900 p-2">
          <span className="text-sm font-extrabold text-white">{"</>"}</span>
        </div>
        <h2 className="font-extrabold">
          <TypeWriter text="Code Sprint" speed={120} />
        </h2>
      </div>
    </Link>
  );
}
