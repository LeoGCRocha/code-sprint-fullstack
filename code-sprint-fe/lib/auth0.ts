import { Auth0Client } from "@auth0/nextjs-auth0/server";
import { NextResponse } from "next/server";

export const auth0 = new Auth0Client({
  authorizationParameters: {
    audience: process.env.AUTH0_AUDIENCE,
  },
  async onCallback(error, ctx) {
    if (error) {
      console.error("[auth0 callback] ERROR:", JSON.stringify(error, Object.getOwnPropertyNames(error), 2));
      return NextResponse.redirect(
        new URL(`/login?error=${encodeURIComponent(error.message)}`, process.env.APP_BASE_URL)
      );
    }
    return NextResponse.redirect(new URL(ctx.returnTo ?? "/", process.env.APP_BASE_URL));
  },
});
