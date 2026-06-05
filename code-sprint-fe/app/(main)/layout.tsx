// TODO: Data fetching strategy per page
// | Problems list (filters/search/pagination) | TanStack Query                          |
// | Problem detail                            | Server Component direct fetch           |
// | User points (Navbar)                      | UserProvider context (already done)     |
// | Submit solution                           | TanStack Query useMutation              |
// | Real-time leaderboard                     | TanStack Query + refetchInterval        |
// | Profile page                              | Server Component direct fetch           |
// | Competition page                          | Server Component direct fetch           |

import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default async function MainLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
