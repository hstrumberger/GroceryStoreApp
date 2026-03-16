import { useQuery } from '@tanstack/react-query';
import { getActiveSales } from '../api/sales';

export const SalesBanner = () => {
  const { data: sales } = useQuery({
    queryKey: ['sales', 'active'],
    queryFn: getActiveSales,
  });

  if (!sales || sales.length === 0) return null;

  return (
    <div style={{ background: '#e53e3e', color: 'white', padding: '12px 24px' }}>
      {sales.map((sale) => (
        <div key={sale.id} style={{ display: 'flex', alignItems: 'center', gap: 16, flexWrap: 'wrap' }}>
          <strong>🏷️ {sale.name}</strong>
          {sale.description && <span>{sale.description}</span>}
          <span style={{ background: 'rgba(255,255,255,0.2)', borderRadius: 4, padding: '2px 8px' }}>
            {sale.discountType === 'Percentage' ? `${sale.discountValue}% OFF` : `$${sale.discountValue} OFF`}
          </span>
          <span style={{ fontSize: 13 }}>
            Ends: {new Date(sale.endDate).toLocaleDateString()}
          </span>
        </div>
      ))}
    </div>
  );
};
