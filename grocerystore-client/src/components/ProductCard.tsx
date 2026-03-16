import { Link } from 'react-router-dom';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import { addToCart } from '../api/cart';
import { useAuthStore } from '../store/authStore';
import { SaleBadge } from './SaleBadge';
import type { Product } from '../types';

interface Props {
  product: Product;
}

export const ProductCard = ({ product }: Props) => {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const queryClient = useQueryClient();
  const primaryImage = product.images.find((i) => i.isPrimary) ?? product.images[0];

  const addMutation = useMutation({
    mutationFn: () => addToCart(product.id, 1),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  });

  const handleAddToCart = () => {
    if (!isAuthenticated) {
      window.location.href = `/login?next=${encodeURIComponent(window.location.pathname)}`;
      return;
    }
    addMutation.mutate();
  };

  const displayPrice = product.discountedPrice ?? product.price;

  return (
    <div style={{ border: '1px solid #e2e8f0', borderRadius: 8, padding: 16, display: 'flex', flexDirection: 'column', gap: 8 }}>
      <Link to={`/products/${product.id}`}>
        {primaryImage ? (
          <img src={primaryImage.url} alt={primaryImage.altText ?? product.name} style={{ width: '100%', height: 180, objectFit: 'cover', borderRadius: 4 }} />
        ) : (
          <div style={{ width: '100%', height: 180, background: '#e2e8f0', borderRadius: 4, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#a0aec0' }}>
            No Image
          </div>
        )}
      </Link>

      <div>
        {product.activeSale && <SaleBadge sale={product.activeSale} />}
        <Link to={`/products/${product.id}`} style={{ textDecoration: 'none', color: 'inherit' }}>
          <h3 style={{ margin: '4px 0', fontSize: 15, fontWeight: 600 }}>{product.name}</h3>
        </Link>
        <p style={{ margin: 0, fontSize: 13, color: '#718096' }}>{product.categoryName}</p>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
        {product.discountedPrice ? (
          <>
            <span style={{ fontWeight: 700, color: '#e53e3e', fontSize: 16 }}>${displayPrice.toFixed(2)}</span>
            <span style={{ textDecoration: 'line-through', color: '#a0aec0', fontSize: 14 }}>${product.price.toFixed(2)}</span>
          </>
        ) : (
          <span style={{ fontWeight: 700, fontSize: 16 }}>${product.price.toFixed(2)}</span>
        )}
      </div>

      <button
        onClick={handleAddToCart}
        disabled={product.stockQuantity === 0 || addMutation.isPending}
        style={{
          background: product.stockQuantity === 0 ? '#a0aec0' : '#3182ce',
          color: 'white',
          border: 'none',
          borderRadius: 6,
          padding: '8px 16px',
          cursor: product.stockQuantity === 0 ? 'not-allowed' : 'pointer',
          fontWeight: 600,
        }}
      >
        {product.stockQuantity === 0 ? 'Out of Stock' : addMutation.isPending ? 'Adding...' : 'Add to Cart'}
      </button>
    </div>
  );
};
