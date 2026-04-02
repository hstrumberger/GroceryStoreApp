import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { checkout } from '../api/orders';
import { getCart } from '../api/cart';

const schema = z.object({
  shippingAddress: z.object({
    firstName: z.string().min(1, 'Required'),
    lastName: z.string().min(1, 'Required'),
    addressLine1: z.string().min(1, 'Required'),
    addressLine2: z.string().optional(),
    city: z.string().min(1, 'Required'),
    state: z.string().min(1, 'Required'),
    postalCode: z.string().min(1, 'Required'),
    country: z.string().min(1, 'Required'),
  }),
  payment: z.object({
    method: z.enum(['CreditCard', 'DebitCard']),
    cardNumber: z.string().length(16, 'Must be 16 digits').regex(/^\d+$/, 'Digits only'),
    cardHolderName: z.string().min(1, 'Required'),
    expiryDate: z.string().regex(/^\d{2}\/\d{2}$/, 'Format: MM/YY'),
    cvv: z.string().min(3).max(4).regex(/^\d+$/, 'Digits only'),
  }),
});

type FormData = z.infer<typeof schema>;

const Field = ({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) => (
  <div style={{ marginBottom: 12 }}>
    <label style={{ display: 'block', marginBottom: 4, fontWeight: 500, fontSize: 14 }}>{label}</label>
    {children}
    {error && <p style={{ color: '#e53e3e', margin: '4px 0 0', fontSize: 12 }}>{error}</p>}
  </div>
);

const inputStyle: React.CSSProperties = { width: '100%', padding: '8px 12px', border: '1px solid #cbd5e0', borderRadius: 6, boxSizing: 'border-box', fontSize: 14 };

export const CheckoutPage = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: cart } = useQuery({ queryKey: ['cart'], queryFn: getCart });

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { payment: { method: 'CreditCard' } },
  });

  const mutation = useMutation({
    mutationFn: checkout,
    onSuccess: (order) => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      navigate(`/orders/${order.id}`);
    },
  });

  const sa = errors.shippingAddress;
  const pe = errors.payment;

  return (
    <div style={{ padding: 24, maxWidth: 900, margin: '0 auto' }}>
      <h1>Checkout</h1>

      <div style={{ display: 'flex', gap: 32, flexWrap: 'wrap' }}>
        <form onSubmit={handleSubmit((d) => mutation.mutate(d))} style={{ flex: 1, minWidth: 300 }}>
          <h2>Shipping Address</h2>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
            <Field label="First Name" error={sa?.firstName?.message}>
              <input {...register('shippingAddress.firstName')} style={inputStyle} />
            </Field>
            <Field label="Last Name" error={sa?.lastName?.message}>
              <input {...register('shippingAddress.lastName')} style={inputStyle} />
            </Field>
          </div>
          <Field label="Address Line 1" error={sa?.addressLine1?.message}>
            <input {...register('shippingAddress.addressLine1')} style={inputStyle} />
          </Field>
          <Field label="Address Line 2 (optional)">
            <input {...register('shippingAddress.addressLine2')} style={inputStyle} />
          </Field>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
            <Field label="City" error={sa?.city?.message}>
              <input {...register('shippingAddress.city')} style={inputStyle} />
            </Field>
            <Field label="State" error={sa?.state?.message}>
              <input {...register('shippingAddress.state')} style={inputStyle} />
            </Field>
            <Field label="Postal Code" error={sa?.postalCode?.message}>
              <input {...register('shippingAddress.postalCode')} style={inputStyle} />
            </Field>
            <Field label="Country" error={sa?.country?.message}>
              <input {...register('shippingAddress.country')} style={inputStyle} defaultValue="US" />
            </Field>
          </div>

          <h2 style={{ marginTop: 24 }}>Payment (Simulated)</h2>
          <Field label="Payment Method">
            <select {...register('payment.method')} style={inputStyle}>
              <option value="CreditCard">Credit Card</option>
              <option value="DebitCard">Debit Card</option>
            </select>
          </Field>
          <Field label="Card Number (16 digits)" error={pe?.cardNumber?.message}>
            <input {...register('payment.cardNumber')} placeholder="1234567812345678" style={inputStyle} maxLength={16} />
          </Field>
          <Field label="Cardholder Name" error={pe?.cardHolderName?.message}>
            <input {...register('payment.cardHolderName')} style={inputStyle} />
          </Field>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0 16px' }}>
            <Field label="Expiry (MM/YY)" error={pe?.expiryDate?.message}>
              <input {...register('payment.expiryDate')} placeholder="12/27" style={inputStyle} maxLength={5} />
            </Field>
            <Field label="CVV" error={pe?.cvv?.message}>
              <input {...register('payment.cvv')} placeholder="123" style={inputStyle} maxLength={4} />
            </Field>
          </div>

          {mutation.isError && (
            <p style={{ color: '#e53e3e', marginTop: 12 }}>
              {(mutation.error as import('axios').AxiosError<{ message?: string }>)?.response?.data?.message ?? 'Checkout failed.'}
            </p>
          )}

          <button
            type="submit"
            disabled={mutation.isPending}
            style={{ marginTop: 16, background: '#38a169', color: 'white', border: 'none', borderRadius: 8, padding: '12px 32px', fontWeight: 600, cursor: 'pointer', fontSize: 16, width: '100%' }}
          >
            {mutation.isPending ? 'Placing Order...' : 'Place Order'}
          </button>
        </form>

        {/* Order Summary */}
        <aside style={{ width: 280, flexShrink: 0 }}>
          <h2>Order Summary</h2>
          {cart?.items.map((item) => (
            <div key={item.id} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8, fontSize: 14 }}>
              <span>{item.productName} × {item.quantity}</span>
              <span>${item.lineTotal.toFixed(2)}</span>
            </div>
          ))}
          <hr />
          <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, marginTop: 8 }}>
            <span>Subtotal</span>
            <span>${cart?.total.toFixed(2)}</span>
          </div>
          <p style={{ color: '#718096', fontSize: 12, marginTop: 8 }}>
            Tax (8%) and shipping calculated at checkout.
          </p>
        </aside>
      </div>
    </div>
  );
};
