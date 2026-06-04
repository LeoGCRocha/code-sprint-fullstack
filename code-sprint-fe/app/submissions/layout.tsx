import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default function SubimissionsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar activeHref="/submit" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
