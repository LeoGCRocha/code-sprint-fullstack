import { auth0 } from "@/lib/auth0";
import { NextRequest, NextResponse } from "next/server";

export async function proxy(request: NextRequest) {
  if (process.env.USE_MOCKS === "true") {
    if (request.nextUrl.pathname === "/auth/logout") {
      const res = NextResponse.redirect(new URL("/", request.url));
      res.cookies.set("mock_logged_out", "true", { path: "/" });
      return res;
    }
    if (request.nextUrl.pathname === "/auth/login" || request.nextUrl.pathname === "/login") {
      const res = NextResponse.redirect(new URL("/", request.url));
      res.cookies.delete("mock_logged_out");
      return res;
    }
    return NextResponse.next();
  }

  const authRes = await auth0.middleware(request);

  if (request.nextUrl.pathname.startsWith("/auth")) {
    return authRes;
  }

  const publicPaths = ["/", "/problems"];
  const isPublic = publicPaths.some(
    (p) => request.nextUrl.pathname === p || request.nextUrl.pathname.startsWith(p + "/")
  );
  if (isPublic) return authRes;

  const session = await auth0.getSession(request);
  if (session && request.nextUrl.pathname === "/login") {
    return NextResponse.redirect(new URL("/", request.url));
  }

  if (!session) {
    return NextResponse.redirect(new URL("/auth/login", request.nextUrl.origin));
  }

  return authRes;
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|mockServiceWorker.js).*)"],
};
