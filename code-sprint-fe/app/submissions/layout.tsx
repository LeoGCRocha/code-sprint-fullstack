import { AppNavbar } from "@/components/layout/AppNavbar";
import { Footer } from "@/components/layout/Footer";

export default function SubimissionsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <AppNavbar activeHref="/submit" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
