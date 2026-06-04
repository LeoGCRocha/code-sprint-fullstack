import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default function ProblemsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar activeHref="/problems" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
