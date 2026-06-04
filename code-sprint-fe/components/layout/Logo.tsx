import Link from "next/link";

export function Logo() {
  return (
    <Link href="/">
      <div className="flex flex-row items-center justify-center gap-2">
        <div className="flex items-center justify-center rounded-xl bg-primary-600 p-2">
          <span className="text-sm font-extrabold text-white">{"</>"}</span>
        </div>
        <h2 className="font-extrabold">Code Sprint</h2>
      </div>
    </Link>
  );
}
