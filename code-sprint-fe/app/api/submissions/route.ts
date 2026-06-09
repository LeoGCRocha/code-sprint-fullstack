import { fowardToApi } from "@/lib/bff";
import { NextRequest } from "next/server";

export async function POST(request: NextRequest) {
  const body = await request.text();
  return fowardToApi("/api/submissions", { method: "POST", body });
}

// TODO: Create the route to assign the WEB SOCKETS....
