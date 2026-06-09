import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";

export default function ProfileLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Navbar activeHref="/profile/me" />
      <main className="flex-1">{children}</main>
      <Footer />
    </>
  );
}
