import { useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Header } from './components/Header';
import { ProtectedRoute } from './components/ProtectedRoute';
import { HomePage } from './pages/HomePage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProductListPage } from './pages/ProductListPage';
import { ProductDetailPage } from './pages/ProductDetailPage';
import { SearchResultsPage } from './pages/SearchResultsPage';
import { CartPage } from './pages/CartPage';
import { CheckoutPage } from './pages/CheckoutPage';
import { OrderConfirmationPage } from './pages/OrderConfirmationPage';
import { useAuthStore } from './store/authStore';
import { refresh, getMe } from './api/auth';
import { setAccessToken } from './api/client';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, staleTime: 1000 * 60 },
  },
});

function AppInitializer({ children }: { children: React.ReactNode }) {
  const { setAuth, clearAuth, accessToken } = useAuthStore();

  useEffect(() => {
    const init = async () => {
      if (accessToken) {
        setAccessToken(accessToken);
        try {
          const user = await getMe();
          setAuth(user, accessToken);
        } catch {
          try {
            const data = await refresh();
            setAuth(data.user, data.accessToken);
          } catch {
            clearAuth();
          }
        }
      } else {
        try {
          const data = await refresh();
          setAuth(data.user, data.accessToken);
        } catch {
          // Not logged in
        }
      }
    };
    init();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return <>{children}</>;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppInitializer>
          <Header />
          <main>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/products" element={<ProductListPage />} />
              <Route path="/products/:id" element={<ProductDetailPage />} />
              <Route path="/search" element={<SearchResultsPage />} />
              <Route
                path="/cart"
                element={<ProtectedRoute><CartPage /></ProtectedRoute>}
              />
              <Route
                path="/checkout"
                element={<ProtectedRoute><CheckoutPage /></ProtectedRoute>}
              />
              <Route
                path="/orders/:id"
                element={<ProtectedRoute><OrderConfirmationPage /></ProtectedRoute>}
              />
            </Routes>
          </main>
        </AppInitializer>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
