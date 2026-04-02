import { render, screen } from '@testing-library/react';
import { SaleBadge } from '../components/SaleBadge';
import type { ActiveSale } from '../types';

const baseSale: ActiveSale = {
  id: 1,
  name: 'Summer Sale',
  discountType: 'Percentage',
  discountValue: 20,
  endDate: '2026-12-31',
};

describe('SaleBadge', () => {
  it('renders percentage discount label', () => {
    render(<SaleBadge sale={baseSale} />);
    expect(screen.getByText('20% OFF')).toBeInTheDocument();
  });

  it('renders fixed amount discount label', () => {
    const sale: ActiveSale = { ...baseSale, discountType: 'FixedAmount', discountValue: 5 };
    render(<SaleBadge sale={sale} />);
    expect(screen.getByText('$5 OFF')).toBeInTheDocument();
  });

  it('renders correct percentage for different value', () => {
    const sale: ActiveSale = { ...baseSale, discountValue: 50 };
    render(<SaleBadge sale={sale} />);
    expect(screen.getByText('50% OFF')).toBeInTheDocument();
  });

  it('renders the badge element with expected styling', () => {
    render(<SaleBadge sale={baseSale} />);
    const badge = screen.getByText('20% OFF');
    expect(badge.tagName).toBe('SPAN');
  });
});
