import { Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { SearchBar } from './SearchBar';
import { useAuthStore } from '../store/authStore';
import { getCart } from '../api/cart';
import { logout } from '../api/auth';

export const Header = () => {
  const { isAuthenticated, user, clearAuth } = useAuthStore();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: cart } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: isAuthenticated,
  });

  const logoutMutation = useMutation({
    mutationFn: logout,
    onSuccess: () => {
      clearAuth();
      queryClient.clear();
      navigate('/');
    },
  });

  const cartItemCount = cart?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0;

  return (
    <header style={{ background: '#2d3748', color: 'white', padding: '12px 24px', display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 16, flexWrap: 'wrap' }}>
      <Link to="/" style={{ color: 'white', textDecoration: 'none', fontSize: 20, fontWeight: 700 }}>
        🛒 GroceryStore
      </Link>

      <nav style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
        <Link to="/products" style={{ color: '#e2e8f0', textDecoration: 'none' }}>Products</Link>
      </nav>

      <SearchBar />

      <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
        {isAuthenticated ? (
          <>
            <span style={{ color: '#e2e8f0', fontSize: 14 }}>Hi, {user?.firstName}</span>
            <Link to="/cart" style={{ color: 'white', textDecoration: 'none', position: 'relative' }}>
              🛒
              {cartItemCount > 0 && (
                <span style={{
                  position: 'absolute', top: -8, right: -8,
                  background: '#e53e3e', borderRadius: '50%',
                  width: 18, height: 18, display: 'flex', alignItems: 'center', justifyContent: 'center',
                  fontSize: 11, fontWeight: 700,
                }}>
                  {cartItemCount}
                </span>
              )}
            </Link>
            <button
              onClick={() => logoutMutation.mutate()}
              style={{ background: 'transparent', border: '1px solid #e2e8f0', color: '#e2e8f0', padding: '4px 12px', borderRadius: 6, cursor: 'pointer' }}
            >
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" style={{ color: '#e2e8f0', textDecoration: 'none' }}>Login</Link>
            <Link to="/register" style={{ color: 'white', background: '#3182ce', padding: '4px 12px', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>Register</Link>
          </>
        )}
      </div>
    </header>
  );
};
