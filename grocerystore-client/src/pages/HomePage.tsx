import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getProducts } from '../api/products';
import { SalesBanner } from '../components/SalesBanner';
import { ProductCard } from '../components/ProductCard';

export const HomePage = () => {
  const { data, isLoading } = useQuery({
    queryKey: ['products', { page: 1, pageSize: 8 }],
    queryFn: () => getProducts({ page: 1, pageSize: 8 }),
  });

  return (
    <div>
      <SalesBanner />
      <div style={{ padding: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
          <h1 style={{ margin: 0 }}>Featured Products</h1>
          <Link to="/products" style={{ color: '#3182ce', textDecoration: 'none' }}>View all →</Link>
        </div>

        {isLoading ? (
          <p>Loading products...</p>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 20 }}>
            {data?.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}

        {!isLoading && (!data?.items.length) && (
          <p style={{ color: '#718096', textAlign: 'center' }}>No products available yet.</p>
        )}
      </div>
    </div>
  );
};
