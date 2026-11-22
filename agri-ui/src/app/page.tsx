import Link from 'next/link';

export default function HomePage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 px-4">
      <div className="bg-white rounded-2xl shadow-md p-10 max-w-md w-full text-center">
        <div className="mb-4">
          <span className="inline-block bg-green-100 text-green-700 rounded-full px-4 py-1 text-sm font-semibold mb-2">
            🌱 Agricultural Platform
          </span>
        </div>
        <h1 className="text-4xl font-bold text-green-600 mb-3">AgriConnect</h1>
        <p className="text-gray-500 mb-8">
          Connecting farmers, suppliers, and agricultural officers on one unified platform.
        </p>
        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <Link
            href="/marketplace"
            className="px-6 py-3 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 transition-colors"
          >
            Browse Marketplace
          </Link>
          <Link
            href="/login"
            className="px-6 py-3 border border-green-600 text-green-600 rounded-lg font-medium hover:bg-green-50 transition-colors"
          >
            Login
          </Link>
        </div>
      </div>
    </div>
  );
}
