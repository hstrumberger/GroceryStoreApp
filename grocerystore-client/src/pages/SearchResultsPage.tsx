import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { search } from '../api/sales';
import { ProductCard } from '../components/ProductCard';
import type { PagedResult, Product } from '../types';

export const SearchResultsPage = () => {
  const [searchParams] = useSearchParams();
  const q = searchParams.get('q') ?? '';
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['search', q, page],
    queryFn: () => search({ q, page, pageSize: 20 }) as Promise<PagedResult<Product>>,
    enabled: !!q,
  });

  return (
    <div style={{ padding: 24 }}>
      <h1>Search results for "{q}"</h1>

      {isLoading ? (
        <p>Searching...</p>
      ) : !data?.items.length ? (
        <p style={{ color: '#718096' }}>No products found.</p>
      ) : (
        <>
          <p style={{ color: '#718096', marginBottom: 16 }}>{data.totalCount} result{data.totalCount !== 1 ? 's' : ''}</p>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 20 }}>
            {data.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>

          {data.totalPages > 1 && (
            <div style={{ display: 'flex', gap: 8, marginTop: 24, justifyContent: 'center' }}>
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>← Prev</button>
              <span>Page {page} of {data.totalPages}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page >= data.totalPages}>Next →</button>
            </div>
          )}
        </>
      )}
    </div>
  );
};
