import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default function CompetetionsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar activeHref="/competitions" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
