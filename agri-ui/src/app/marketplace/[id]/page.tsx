import Link from 'next/link';

const crops: Record<string, { name: string; description: string; farmer: string; email: string; price: string }> = {
  '1': {
    name: 'Organic Tomatoes',
    description: 'Freshly harvested organic tomatoes grown without pesticides. Perfect for cooking and salads.',
    farmer: 'Ravi Kumar',
    email: 'ravi.kumar@agriconnect.lk',
    price: 'LKR 120/kg',
  },
  '2': {
    name: 'Basmati Rice',
    description: 'Premium long-grain basmati rice, sun-dried and hand-processed for the finest quality.',
    farmer: 'Priya Devi',
    email: 'priya.devi@agriconnect.lk',
    price: 'LKR 250/kg',
  },
  '3': {
    name: 'Fresh Spinach',
    description: 'Tender, nutrient-rich spinach leaves harvested daily. Rich in iron and vitamins.',
    farmer: 'Suresh Patel',
    email: 'suresh.patel@agriconnect.lk',
    price: 'LKR 80/kg',
  },
};

export default async function CropDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const crop = crops[id];

  if (!crop) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
        <div className="bg-white rounded-xl shadow p-8 text-center max-w-sm w-full">
          <p className="text-gray-500 mb-4">Crop not found.</p>
          <Link href="/marketplace" className="text-green-600 hover:underline">
            ← Back to Marketplace
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 px-4 py-10">
      <div className="max-w-lg mx-auto">
        <Link href="/marketplace" className="text-green-600 hover:underline text-sm mb-6 inline-block">
          ← Back to Marketplace
        </Link>
        <div className="bg-white rounded-2xl shadow p-8">
          <h1 className="text-2xl font-bold text-gray-800 mb-2">{crop.name}</h1>
          <span className="bg-green-50 text-green-700 font-semibold px-3 py-1 rounded-lg text-sm inline-block mb-4">
            {crop.price}
          </span>
          <p className="text-gray-600 mb-6">{crop.description}</p>
          <div className="border-t pt-4">
            <p className="text-sm text-gray-500 font-medium">Farmer</p>
            <p className="text-gray-800 font-semibold">{crop.farmer}</p>
            <p className="text-green-600 text-sm mt-1">{crop.email}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
