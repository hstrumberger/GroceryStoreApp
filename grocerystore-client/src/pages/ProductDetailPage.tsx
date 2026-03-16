import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getProduct } from '../api/products';
import { addToCart } from '../api/cart';
import { useAuthStore } from '../store/authStore';
import { SaleBadge } from '../components/SaleBadge';

export const ProductDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const navigate = useNavigate();
  const [qty, setQty] = useState(1);
  const [selectedImage, setSelectedImage] = useState(0);

  const { data: product, isLoading, error } = useQuery({
    queryKey: ['product', id],
    queryFn: () => getProduct(Number(id)),
    enabled: !!id,
  });

  const addMutation = useMutation({
    mutationFn: () => addToCart(Number(id), qty),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      navigate('/cart');
    },
  });

  if (isLoading) return <p style={{ padding: 24 }}>Loading...</p>;
  if (error || !product) return <p style={{ padding: 24 }}>Product not found.</p>;

  const images = product.images.sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <div style={{ padding: 24, maxWidth: 900, margin: '0 auto' }}>
      <div style={{ display: 'flex', gap: 32, flexWrap: 'wrap' }}>
        {/* Images */}
        <div style={{ flex: '0 0 380px' }}>
          {images.length > 0 ? (
            <>
              <img src={images[selectedImage].url} alt={images[selectedImage].altText ?? product.name} style={{ width: '100%', height: 300, objectFit: 'cover', borderRadius: 8 }} />
              <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
                {images.map((img, i) => (
                  <img key={img.id} src={img.url} alt={img.altText ?? ''} onClick={() => setSelectedImage(i)}
                    style={{ width: 60, height: 60, objectFit: 'cover', borderRadius: 4, cursor: 'pointer', border: i === selectedImage ? '2px solid #3182ce' : '2px solid transparent' }} />
                ))}
              </div>
            </>
          ) : (
            <div style={{ width: '100%', height: 300, background: '#e2e8f0', borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#a0aec0' }}>
              No Image
            </div>
          )}
        </div>

        {/* Info */}
        <div style={{ flex: 1, minWidth: 280 }}>
          <p style={{ margin: '0 0 4px', color: '#718096', fontSize: 13 }}>{product.categoryName}</p>
          <h1 style={{ margin: '0 0 8px', fontSize: 24 }}>{product.name}</h1>
          <p style={{ margin: '0 0 8px', color: '#718096', fontSize: 13 }}>SKU: {product.sku}</p>

          {product.activeSale && <div style={{ marginBottom: 8 }}><SaleBadge sale={product.activeSale} /></div>}

          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
            {product.discountedPrice ? (
              <>
                <span style={{ fontSize: 28, fontWeight: 700, color: '#e53e3e' }}>${product.discountedPrice.toFixed(2)}</span>
                <span style={{ textDecoration: 'line-through', color: '#a0aec0', fontSize: 20 }}>${product.price.toFixed(2)}</span>
              </>
            ) : (
              <span style={{ fontSize: 28, fontWeight: 700 }}>${product.price.toFixed(2)}</span>
            )}
          </div>

          {product.description && <p style={{ marginBottom: 16, color: '#4a5568', lineHeight: 1.6 }}>{product.description}</p>}

          <p style={{ color: product.stockQuantity > 0 ? '#38a169' : '#e53e3e', fontWeight: 600, marginBottom: 16 }}>
            {product.stockQuantity > 0 ? `In Stock (${product.stockQuantity})` : 'Out of Stock'}
          </p>

          {product.stockQuantity > 0 && (
            <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16 }}>
              <label>Qty:</label>
              <input type="number" min={1} max={product.stockQuantity} value={qty} onChange={(e) => setQty(Number(e.target.value))}
                style={{ width: 60, padding: '6px 8px', border: '1px solid #cbd5e0', borderRadius: 6 }} />
            </div>
          )}

          <button
            onClick={() => {
              if (!isAuthenticated) {
                navigate(`/login?next=/products/${id}`);
                return;
              }
              addMutation.mutate();
            }}
            disabled={product.stockQuantity === 0 || addMutation.isPending}
            style={{ background: product.stockQuantity === 0 ? '#a0aec0' : '#3182ce', color: 'white', border: 'none', borderRadius: 8, padding: '12px 24px', fontWeight: 600, cursor: 'pointer', fontSize: 16 }}
          >
            {addMutation.isPending ? 'Adding...' : 'Add to Cart'}
          </button>

          {product.manufacturerName && (
            <p style={{ marginTop: 16, color: '#718096', fontSize: 13 }}>Brand: {product.manufacturerName}</p>
          )}
          {product.averageRating > 0 && (
            <p style={{ color: '#718096', fontSize: 13 }}>Rating: {product.averageRating.toFixed(1)} ★ ({product.ratingCount} reviews)</p>
          )}
        </div>
      </div>
    </div>
  );
};
