import type { ActiveSale } from '../types';

interface Props {
  sale: ActiveSale;
}

export const SaleBadge = ({ sale }: Props) => {
  const label = sale.discountType === 'Percentage'
    ? `${sale.discountValue}% OFF`
    : `$${sale.discountValue} OFF`;

  return (
    <span style={{
      background: '#e53e3e',
      color: 'white',
      padding: '2px 8px',
      borderRadius: 4,
      fontSize: 12,
      fontWeight: 'bold',
    }}>
      {label}
    </span>
  );
};
