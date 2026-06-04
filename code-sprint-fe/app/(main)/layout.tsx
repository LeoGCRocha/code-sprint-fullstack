import { Navbar } from "@/components/layout/Navbar";
import { Footer } from "@/components/layout/Footer";
import { UserProvider } from "@/provider/UserProvider";
import { getCurrentUser } from "@/services/users";

export default async function MainLayout({ children }: { children: React.ReactNode }) {
  const user = await getCurrentUser();

  return (
    <UserProvider initialUser={user}>
      <Navbar />
      <main className="flex-1">{children}</main>
      <Footer />
    </UserProvider>
  );
}
