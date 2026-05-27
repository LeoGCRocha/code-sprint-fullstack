export function Footer() {
  const getCurrentYear = new Date().getFullYear();
  return (
    <footer className="mt-2 text-center text-sm">
      {`© ${getCurrentYear} Code Sprint Platform. All rights reserved.`}
    </footer>
  );
}
