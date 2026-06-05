import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Footer } from "@/components/layout/Footer";
import { getCurrentUser } from "@/services/users";
import { UserProvider } from "@/provider/UserProvider";
import { ServerErrorOverlay } from "@/components/layout/ServerErrorOverlay";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { MockProvider } from "@/components/providers/MockProvider";

const useMocks = process.env.NEXT_PUBLIC_USE_MOCKS === "true";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Problem Solver",
  description: "Competitive programming platform",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const result = await getCurrentUser();

  const user = result.state === "ok" ? result.user : null;

  return (
    <html lang="en" className={`${inter.variable} h-full antialiased`}>
      <body className="bg-background flex min-h-full flex-col overflow-x-hidden">
        <QueryProvider>
          <UserProvider initialUser={user}>
            {result.state === "error" && <ServerErrorOverlay />}
            {useMocks ? <MockProvider>{children}</MockProvider> : children}
          </UserProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
