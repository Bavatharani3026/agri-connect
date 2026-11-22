import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'AgriConnect',
  description: 'Connecting farmers, suppliers, and officers',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-gray-50 font-sans antialiased">
        {children}
      </body>
    </html>
  );
}
