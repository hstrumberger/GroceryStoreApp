import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getCart, updateCartItem, removeCartItem, clearCart } from '../api/cart';
import { SaleBadge } from '../components/SaleBadge';

export const CartPage = () => {
  const queryClient = useQueryClient();

  const { data: cart, isLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
  });

  const updateMutation = useMutation({
    mutationFn: ({ productId, quantity }: { productId: number; quantity: number }) =>
      updateCartItem(productId, quantity),
    onSuccess: (data) => queryClient.setQueryData(['cart'], data),
  });

  const removeMutation = useMutation({
    mutationFn: removeCartItem,
    onSuccess: (data) => queryClient.setQueryData(['cart'], data),
  });

  const clearMutation = useMutation({
    mutationFn: clearCart,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  });

  if (isLoading) return <p style={{ padding: 24 }}>Loading cart...</p>;

  const items = cart?.items ?? [];

  return (
    <div style={{ padding: 24, maxWidth: 800, margin: '0 auto' }}>
      <h1>Your Cart</h1>

      {items.length === 0 ? (
        <div style={{ textAlign: 'center', padding: 48 }}>
          <p style={{ color: '#718096', fontSize: 18 }}>Your cart is empty.</p>
          <Link to="/products" style={{ color: '#3182ce' }}>Browse products</Link>
        </div>
      ) : (
        <>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid #e2e8f0' }}>
                <th style={{ textAlign: 'left', padding: '8px 0' }}>Product</th>
                <th style={{ textAlign: 'center', padding: '8px 0' }}>Price</th>
                <th style={{ textAlign: 'center', padding: '8px 0' }}>Quantity</th>
                <th style={{ textAlign: 'right', padding: '8px 0' }}>Total</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id} style={{ borderBottom: '1px solid #e2e8f0' }}>
                  <td style={{ padding: '12px 0' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                      {item.imageUrl && <img src={item.imageUrl} alt={item.productName} style={{ width: 60, height: 60, objectFit: 'cover', borderRadius: 4 }} />}
                      <div>
                        <Link to={`/products/${item.productId}`} style={{ fontWeight: 600, textDecoration: 'none', color: 'inherit' }}>{item.productName}</Link>
                        {item.activeSale && <div style={{ marginTop: 4 }}><SaleBadge sale={item.activeSale} /></div>}
                      </div>
                    </div>
                  </td>
                  <td style={{ textAlign: 'center', padding: '12px 8px' }}>
                    {item.discountedPrice ? (
                      <div>
                        <span style={{ color: '#e53e3e', fontWeight: 600 }}>${item.discountedPrice.toFixed(2)}</span>
                        <br />
                        <span style={{ textDecoration: 'line-through', color: '#a0aec0', fontSize: 12 }}>${item.unitPrice.toFixed(2)}</span>
                      </div>
                    ) : (
                      <span>${item.unitPrice.toFixed(2)}</span>
                    )}
                  </td>
                  <td style={{ textAlign: 'center', padding: '12px 8px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8 }}>
                      <button
                        onClick={() => {
                          if (item.quantity === 1) removeMutation.mutate(item.productId);
                          else updateMutation.mutate({ productId: item.productId, quantity: item.quantity - 1 });
                        }}
                        style={{ width: 28, height: 28, border: '1px solid #cbd5e0', borderRadius: 4, cursor: 'pointer', background: 'white' }}
                      >−</button>
                      <span>{item.quantity}</span>
                      <button
                        onClick={() => updateMutation.mutate({ productId: item.productId, quantity: item.quantity + 1 })}
                        disabled={item.quantity >= item.stockQuantity}
                        style={{ width: 28, height: 28, border: '1px solid #cbd5e0', borderRadius: 4, cursor: 'pointer', background: 'white' }}
                      >+</button>
                    </div>
                  </td>
                  <td style={{ textAlign: 'right', padding: '12px 8px', fontWeight: 600 }}>${item.lineTotal.toFixed(2)}</td>
                  <td style={{ padding: '12px 0 12px 8px' }}>
                    <button onClick={() => removeMutation.mutate(item.productId)} style={{ background: 'none', border: 'none', color: '#e53e3e', cursor: 'pointer', fontSize: 18 }}>✕</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginTop: 24 }}>
            <button
              onClick={() => clearMutation.mutate()}
              style={{ background: 'none', border: '1px solid #e53e3e', color: '#e53e3e', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}
            >
              Clear Cart
            </button>

            <div style={{ textAlign: 'right' }}>
              <div style={{ fontSize: 20, fontWeight: 700, marginBottom: 16 }}>
                Total: ${cart?.total.toFixed(2)}
              </div>
              <Link to="/checkout">
                <button style={{ background: '#38a169', color: 'white', border: 'none', borderRadius: 8, padding: '12px 32px', fontWeight: 600, cursor: 'pointer', fontSize: 16 }}>
                  Proceed to Checkout →
                </button>
              </Link>
            </div>
          </div>
        </>
      )}
    </div>
  );
};
