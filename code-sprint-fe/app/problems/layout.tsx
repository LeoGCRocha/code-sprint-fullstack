import { ProblemsNavbar } from "@/features/problems/components/ProblemsNavbar";

export default function ProblemsLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <ProblemsNavbar />
      <main>{children}</main>
    </>
  );
}
