import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { getProducts, getCategories } from '../api/products';
import { ProductCard } from '../components/ProductCard';

export const ProductListPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const categoryId = searchParams.get('categoryId') ? Number(searchParams.get('categoryId')) : undefined;
  const sortBy = searchParams.get('sortBy') ?? undefined;

  const { data, isLoading } = useQuery({
    queryKey: ['products', { categoryId, page, sortBy }],
    queryFn: () => getProducts({ categoryId, page, pageSize: 20, sortBy }),
  });

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });

  const updateParam = (key: string, value: string | undefined) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value);
    else next.delete(key);
    setSearchParams(next);
    setPage(1);
  };

  return (
    <div style={{ display: 'flex', gap: 24, padding: 24 }}>
      {/* Sidebar */}
      <aside style={{ width: 200, flexShrink: 0 }}>
        <h3 style={{ marginTop: 0 }}>Categories</h3>
        <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
          <li>
            <button
              onClick={() => updateParam('categoryId', undefined)}
              style={{ background: 'none', border: 'none', cursor: 'pointer', padding: '4px 0', fontWeight: !categoryId ? 700 : 400, color: !categoryId ? '#3182ce' : 'inherit' }}
            >
              All Products
            </button>
          </li>
          {categories?.map((cat) => (
            <li key={cat.id}>
              <button
                onClick={() => updateParam('categoryId', String(cat.id))}
                style={{ background: 'none', border: 'none', cursor: 'pointer', padding: '4px 0', fontWeight: categoryId === cat.id ? 700 : 400, color: categoryId === cat.id ? '#3182ce' : 'inherit' }}
              >
                {cat.name}
              </button>
              {cat.subCategories.map((sub) => (
                <div key={sub.id} style={{ paddingLeft: 12 }}>
                  <button
                    onClick={() => updateParam('categoryId', String(sub.id))}
                    style={{ background: 'none', border: 'none', cursor: 'pointer', padding: '2px 0', fontSize: 13, color: categoryId === sub.id ? '#3182ce' : '#718096' }}
                  >
                    {sub.name}
                  </button>
                </div>
              ))}
            </li>
          ))}
        </ul>

        <h3>Sort By</h3>
        {['name', 'price_asc', 'price_desc', 'rating'].map((opt) => (
          <div key={opt}>
            <button
              onClick={() => updateParam('sortBy', opt)}
              style={{ background: 'none', border: 'none', cursor: 'pointer', padding: '4px 0', fontWeight: sortBy === opt ? 700 : 400, color: sortBy === opt ? '#3182ce' : 'inherit' }}
            >
              {opt === 'name' ? 'Name' : opt === 'price_asc' ? 'Price: Low to High' : opt === 'price_desc' ? 'Price: High to Low' : 'Rating'}
            </button>
          </div>
        ))}
      </aside>

      {/* Product grid */}
      <main style={{ flex: 1 }}>
        <h1 style={{ marginTop: 0 }}>Products {data && `(${data.totalCount})`}</h1>

        {isLoading ? (
          <p>Loading...</p>
        ) : (
          <>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 20 }}>
              {data?.items.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>

            {data && data.totalPages > 1 && (
              <div style={{ display: 'flex', gap: 8, marginTop: 24, justifyContent: 'center' }}>
                <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>← Prev</button>
                <span>Page {page} of {data.totalPages}</span>
                <button onClick={() => setPage((p) => p + 1)} disabled={page >= data.totalPages}>Next →</button>
              </div>
            )}
          </>
        )}
      </main>
    </div>
  );
};
