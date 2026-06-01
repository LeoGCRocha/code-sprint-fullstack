import { AppNavbar } from "@/components/layout/AppNavbar";
import { Footer } from "@/components/layout/Footer";

export default function CompetetionsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <AppNavbar activeHref="/competitions" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
