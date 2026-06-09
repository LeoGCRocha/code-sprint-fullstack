import { fowardToApi } from "@/lib/bff";
import { NextRequest } from "next/server";

export async function GET(_request: NextRequest, { params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  return fowardToApi(`/api/submissions/${encodeURIComponent(id)}`);
}
