import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { SearchBar } from '../components/SearchBar';

const mockNavigate = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

function renderSearchBar() {
  return render(
    <MemoryRouter>
      <SearchBar />
    </MemoryRouter>
  );
}

describe('SearchBar', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders input and search button', () => {
    renderSearchBar();
    expect(screen.getByPlaceholderText('Search products...')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /search/i })).toBeInTheDocument();
  });

  it('navigates to search results on submit', async () => {
    renderSearchBar();
    const input = screen.getByPlaceholderText('Search products...');

    await userEvent.type(input, 'apple');
    await userEvent.click(screen.getByRole('button', { name: /search/i }));

    expect(mockNavigate).toHaveBeenCalledWith('/search?q=apple');
  });

  it('does not navigate when query is empty', async () => {
    renderSearchBar();
    await userEvent.click(screen.getByRole('button', { name: /search/i }));
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('does not navigate when query is only whitespace', async () => {
    renderSearchBar();
    const input = screen.getByPlaceholderText('Search products...');

    await userEvent.type(input, '   ');
    await userEvent.click(screen.getByRole('button', { name: /search/i }));

    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('encodes special characters in the search query', async () => {
    renderSearchBar();
    const input = screen.getByPlaceholderText('Search products...');

    await userEvent.type(input, 'green & red peppers');
    await userEvent.click(screen.getByRole('button', { name: /search/i }));

    expect(mockNavigate).toHaveBeenCalledWith(
      `/search?q=${encodeURIComponent('green & red peppers')}`
    );
  });

  it('trims whitespace from search query before navigating', async () => {
    renderSearchBar();
    const input = screen.getByPlaceholderText('Search products...');

    await userEvent.type(input, '  banana  ');
    await userEvent.click(screen.getByRole('button', { name: /search/i }));

    expect(mockNavigate).toHaveBeenCalledWith('/search?q=banana');
  });
});
