export function Footer() {
  return (
    <footer suppressHydrationWarning className="mt-2 text-center text-sm">
      {`© ${new Date().getFullYear()} Code Sprint Platform. All rights reserved.`}
    </footer>
  );
}
