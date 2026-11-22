'use client';

import { useEffect, useState } from 'react';

interface User {
  id: string;
  email: string;
  full_name: string;
  role: string;
}

export default function SupplierDashboard() {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const stored = localStorage.getItem('user');
    if (!stored) {
      window.location.href = '/login';
      return;
    }
    const parsed: User = JSON.parse(stored);
    if (parsed.role !== 'supplier') {
      window.location.href = '/login';
      return;
    }
    setUser(parsed);
  }, []);

  const handleLogout = () => {
    localStorage.clear();
    window.location.href = '/';
  };

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-400">Loading…</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-md p-8 max-w-sm w-full">
        <div className="mb-4">
          <span className="inline-block bg-blue-100 text-blue-700 rounded-full px-3 py-1 text-xs font-semibold">
            📦 Supplier
          </span>
        </div>
        <h1 className="text-2xl font-bold text-gray-800 mb-1">Welcome, {user.full_name}!</h1>
        <p className="text-gray-500 text-sm mb-1">Role: <span className="font-medium text-blue-600">Supplier</span></p>
        <p className="text-gray-400 text-xs mb-6">{user.email}</p>

        <div className="bg-blue-50 rounded-xl p-4 mb-6 text-sm text-gray-600">
          Your supplier dashboard is active. You can manage your supply listings here.
        </div>

        <button
          onClick={handleLogout}
          className="w-full border border-red-300 text-red-600 py-2 rounded-lg font-medium hover:bg-red-50 transition-colors"
        >
          Logout
        </button>
      </div>
    </div>
  );
}
