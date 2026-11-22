import Link from 'next/link';

const crops = [
  { id: '1', name: 'Organic Tomatoes', farmer: 'Ravi Kumar', price: 'LKR 120/kg' },
  { id: '2', name: 'Basmati Rice', farmer: 'Priya Devi', price: 'LKR 250/kg' },
  { id: '3', name: 'Fresh Spinach', farmer: 'Suresh Patel', price: 'LKR 80/kg' },
];

export default function MarketplacePage() {
  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-2xl mx-auto">
        <Link href="/" className="text-green-600 hover:underline text-sm mb-6 inline-block">
          ← Back to Home
        </Link>
        <h1 className="text-3xl font-bold text-gray-800 mb-2">Crop Marketplace</h1>
        <p className="text-gray-500 mb-8">Browse fresh produce directly from farmers.</p>

        <div className="flex flex-col gap-4">
          {crops.map((crop) => (
            <Link key={crop.id} href={`/marketplace/${crop.id}`}>
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow cursor-pointer">
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-lg font-semibold text-gray-800">{crop.name}</h2>
                    <p className="text-gray-500 text-sm mt-1">Farmer: {crop.farmer}</p>
                  </div>
                  <span className="bg-green-50 text-green-700 font-semibold px-3 py-1 rounded-lg text-sm">
                    {crop.price}
                  </span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
