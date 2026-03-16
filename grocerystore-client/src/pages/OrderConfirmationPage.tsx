import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getOrder } from '../api/orders';

export const OrderConfirmationPage = () => {
  const { id } = useParams<{ id: string }>();

  const { data: order, isLoading, error } = useQuery({
    queryKey: ['order', id],
    queryFn: () => getOrder(Number(id)),
    enabled: !!id,
  });

  if (isLoading) return <p style={{ padding: 24 }}>Loading...</p>;
  if (error || !order) return <p style={{ padding: 24 }}>Order not found.</p>;

  return (
    <div style={{ padding: 24, maxWidth: 700, margin: '0 auto' }}>
      <div style={{ textAlign: 'center', marginBottom: 32 }}>
        <div style={{ fontSize: 64 }}>✅</div>
        <h1 style={{ color: '#38a169' }}>Order Confirmed!</h1>
        <p style={{ color: '#718096' }}>Order #{order.id} — {new Date(order.createdAt).toLocaleString()}</p>
      </div>

      <div style={{ background: '#f7fafc', border: '1px solid #e2e8f0', borderRadius: 8, padding: 24, marginBottom: 24 }}>
        <h3 style={{ marginTop: 0 }}>Order Items</h3>
        {order.items.map((item) => (
          <div key={item.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '8px 0', borderBottom: '1px solid #e2e8f0' }}>
            <span>{item.productName} × {item.quantity}</span>
            <span>${item.lineTotal.toFixed(2)}</span>
          </div>
        ))}

        <div style={{ marginTop: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', color: '#718096' }}>
            <span>Subtotal</span><span>${order.subTotal.toFixed(2)}</span>
          </div>
          {order.discountAmount > 0 && (
            <div style={{ display: 'flex', justifyContent: 'space-between', color: '#38a169' }}>
              <span>Discount</span><span>-${order.discountAmount.toFixed(2)}</span>
            </div>
          )}
          <div style={{ display: 'flex', justifyContent: 'space-between', color: '#718096' }}>
            <span>Tax</span><span>${order.taxAmount.toFixed(2)}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', color: '#718096' }}>
            <span>Shipping</span><span>{order.shippingAmount === 0 ? 'FREE' : `$${order.shippingAmount.toFixed(2)}`}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, fontSize: 18, marginTop: 8 }}>
            <span>Total</span><span>${order.total.toFixed(2)}</span>
          </div>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 24 }}>
        <div>
          <h3 style={{ marginTop: 0 }}>Shipping To</h3>
          <p style={{ margin: 0 }}>
            {order.shippingAddress.firstName} {order.shippingAddress.lastName}<br />
            {order.shippingAddress.addressLine1}<br />
            {order.shippingAddress.addressLine2 && <>{order.shippingAddress.addressLine2}<br /></>}
            {order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.postalCode}<br />
            {order.shippingAddress.country}
          </p>
        </div>
        <div>
          <h3 style={{ marginTop: 0 }}>Payment</h3>
          <p style={{ margin: 0 }}>
            {order.paymentMethod}<br />
            {order.paymentLast4 && <>Card ending in {order.paymentLast4}</>}
          </p>
        </div>
      </div>

      <div style={{ textAlign: 'center', display: 'flex', gap: 16, justifyContent: 'center' }}>
        <Link to="/">
          <button style={{ background: '#3182ce', color: 'white', border: 'none', borderRadius: 8, padding: '10px 24px', fontWeight: 600, cursor: 'pointer' }}>
            Continue Shopping
          </button>
        </Link>
        <Link to="/orders">
          <button style={{ background: 'white', color: '#3182ce', border: '1px solid #3182ce', borderRadius: 8, padding: '10px 24px', fontWeight: 600, cursor: 'pointer' }}>
            View All Orders
          </button>
        </Link>
      </div>
    </div>
  );
};
