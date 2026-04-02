import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { ProductCard } from '../components/ProductCard';
import type { Product } from '../types';

vi.mock('../store/authStore', () => ({
  useAuthStore: vi.fn(() => false),
}));

vi.mock('../api/cart', () => ({
  addToCart: vi.fn().mockResolvedValue({}),
}));

const baseProduct: Product = {
  id: 1,
  sku: 'APP-001',
  name: 'Apple',
  price: 2.99,
  categoryId: 1,
  categoryName: 'Fruits',
  averageRating: 4.5,
  ratingCount: 10,
  stockQuantity: 100,
  isActive: true,
  images: [],
};

function renderCard(product: Product = baseProduct) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <ProductCard product={product} />
      </QueryClientProvider>
    </MemoryRouter>
  );
}

describe('ProductCard', () => {
  it('renders product name and price', () => {
    renderCard();
    expect(screen.getByText('Apple')).toBeInTheDocument();
    expect(screen.getByText('$2.99')).toBeInTheDocument();
  });

  it('renders category name', () => {
    renderCard();
    expect(screen.getByText('Fruits')).toBeInTheDocument();
  });

  it('shows Add to Cart button when in stock', () => {
    renderCard();
    expect(screen.getByRole('button', { name: /add to cart/i })).toBeInTheDocument();
  });

  it('shows Out of Stock and disables button when stockQuantity is 0', () => {
    renderCard({ ...baseProduct, stockQuantity: 0 });
    const button = screen.getByRole('button', { name: /out of stock/i });
    expect(button).toBeInTheDocument();
    expect(button).toBeDisabled();
  });

  it('shows discounted price with original strikethrough when on sale', () => {
    renderCard({
      ...baseProduct,
      discountedPrice: 1.99,
      activeSale: {
        id: 1,
        name: 'Summer Sale',
        discountType: 'Percentage',
        discountValue: 33,
        endDate: '2026-12-31',
      },
    });
    expect(screen.getByText('$1.99')).toBeInTheDocument();
    expect(screen.getByText('$2.99')).toBeInTheDocument();
  });

  it('shows sale badge when product has an active sale', () => {
    renderCard({
      ...baseProduct,
      discountedPrice: 1.99,
      activeSale: {
        id: 1,
        name: 'Summer Sale',
        discountType: 'Percentage',
        discountValue: 33,
        endDate: '2026-12-31',
      },
    });
    expect(screen.getByText('33% OFF')).toBeInTheDocument();
  });

  it('renders No Image placeholder when product has no images', () => {
    renderCard();
    expect(screen.getByText('No Image')).toBeInTheDocument();
  });

  it('renders product image when images are provided', () => {
    renderCard({
      ...baseProduct,
      images: [
        { id: 1, url: 'https://example.com/apple.jpg', altText: 'Red Apple', displayOrder: 1, isPrimary: true },
      ],
    });
    const img = screen.getByRole('img', { name: 'Red Apple' });
    expect(img).toBeInTheDocument();
    expect(img).toHaveAttribute('src', 'https://example.com/apple.jpg');
  });

  it('links to product detail page', () => {
    renderCard();
    const links = screen.getAllByRole('link');
    expect(links.some((l) => l.getAttribute('href') === '/products/1')).toBe(true);
  });
});
