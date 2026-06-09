import { Container } from "@/components/ui/Container";
import { BeUser } from "@/services/users";

export type ProfileProps = {
  user: BeUser;
};

function formatMemberSince(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("en-US", { month: "short", year: "numeric" });
}

export function Profile({ user }: ProfileProps) {
  const initials = user.displayName
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <Container className="mt-0 max-w-full">
      <div className="flex flex-col gap-3">
        <div className="flex flex-row gap-3">
          <div className="bg-primary-600 relative flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-full">
            {user.avatar ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={user.avatar} alt={user.displayName} className="h-full w-full object-cover" />
            ) : (
              <span className="text-2xl font-semibold text-white">{initials}</span>
            )}
          </div>
          <div>
            <h1 className="leading-tight font-black">{user.displayName}</h1>
            <span className="text-sm text-neutral-500">@{user.handle}</span>
          </div>
        </div>

        <span className="text-xs text-neutral-400">
          Member since {formatMemberSince(user.memberSince)}
        </span>
      </div>
    </Container>
  );
}
