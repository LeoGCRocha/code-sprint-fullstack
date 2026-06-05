"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import React from "react";

const client = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60_000, // data is considered fresh for 60s - no background refetch during this window
      retry: 2,
    },
  },
});

export type QueryProviderProps = {
  children: React.ReactNode;
};

export function QueryProvider({ children }: QueryProviderProps) {
  return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
}
