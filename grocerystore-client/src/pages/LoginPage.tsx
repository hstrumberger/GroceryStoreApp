import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { login } from '../api/auth';
import { useAuthStore } from '../store/authStore';

const schema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(1, 'Password required'),
});

type FormData = z.infer<typeof schema>;

export const LoginPage = () => {
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const next = searchParams.get('next') ?? '/';

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken);
      navigate(next, { replace: true });
    },
  });

  return (
    <div style={{ maxWidth: 400, margin: '80px auto', padding: 24, border: '1px solid #e2e8f0', borderRadius: 8 }}>
      <h2 style={{ textAlign: 'center', marginBottom: 24 }}>Sign In</h2>
      <form onSubmit={handleSubmit((d) => mutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
        <div>
          <label style={{ display: 'block', marginBottom: 4, fontWeight: 500 }}>Email</label>
          <input {...register('email')} type="email" style={{ width: '100%', padding: '8px 12px', border: '1px solid #cbd5e0', borderRadius: 6, boxSizing: 'border-box' }} />
          {errors.email && <p style={{ color: '#e53e3e', margin: '4px 0 0', fontSize: 13 }}>{errors.email.message}</p>}
        </div>
        <div>
          <label style={{ display: 'block', marginBottom: 4, fontWeight: 500 }}>Password</label>
          <input {...register('password')} type="password" style={{ width: '100%', padding: '8px 12px', border: '1px solid #cbd5e0', borderRadius: 6, boxSizing: 'border-box' }} />
          {errors.password && <p style={{ color: '#e53e3e', margin: '4px 0 0', fontSize: 13 }}>{errors.password.message}</p>}
        </div>
        {mutation.isError && (
          <p style={{ color: '#e53e3e', textAlign: 'center', margin: 0 }}>
            {(mutation.error as any)?.response?.data?.message ?? 'Login failed.'}
          </p>
        )}
        <button
          type="submit"
          disabled={mutation.isPending}
          style={{ background: '#3182ce', color: 'white', border: 'none', borderRadius: 6, padding: '10px', fontWeight: 600, cursor: mutation.isPending ? 'wait' : 'pointer', fontSize: 16 }}
        >
          {mutation.isPending ? 'Signing in...' : 'Sign In'}
        </button>
      </form>
      <p style={{ textAlign: 'center', marginTop: 16 }}>
        Don't have an account? <Link to="/register">Register</Link>
      </p>
    </div>
  );
};
