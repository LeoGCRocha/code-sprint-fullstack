import { auth0 } from "@/lib/auth0";
import { NextRequest, NextResponse } from "next/server";

export async function proxy(request: NextRequest) {
  const authRes = await auth0.middleware(request);

  if (request.nextUrl.pathname.startsWith("/auth")) {
    return authRes;
  }

  const publicPaths = ["/", "/login", "/problems"];
  const isPublic = publicPaths.some(
    (p) => request.nextUrl.pathname === p || request.nextUrl.pathname.startsWith(p + "/")
  );
  if (isPublic) return authRes;

  const session = await auth0.getSession(request);

  if (!session) {
    return NextResponse.redirect(new URL("/auth/login", request.nextUrl.origin));
  }

  return authRes;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
