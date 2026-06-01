import { AppNavbar } from "@/components/layout/AppNavbar";
import { Footer } from "@/components/layout/Footer";

export default function ProfileLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <AppNavbar activeHref="/profile" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
