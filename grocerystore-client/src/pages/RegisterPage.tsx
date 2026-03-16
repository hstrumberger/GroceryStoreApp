import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { useNavigate, Link } from 'react-router-dom';
import { register as registerUser } from '../api/auth';
import { useAuthStore } from '../store/authStore';

const schema = z.object({
  firstName: z.string().min(1, 'First name required'),
  lastName: z.string().min(1, 'Last name required'),
  email: z.string().email('Invalid email'),
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Must contain an uppercase letter')
    .regex(/[0-9]/, 'Must contain a number')
    .regex(/[^a-zA-Z0-9]/, 'Must contain a special character'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] });

type FormData = z.infer<typeof schema>;

export const RegisterPage = () => {
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const mutation = useMutation({
    mutationFn: registerUser,
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken);
      navigate('/');
    },
  });

  const fields: { name: keyof FormData; label: string; type: string }[] = [
    { name: 'firstName', label: 'First Name', type: 'text' },
    { name: 'lastName', label: 'Last Name', type: 'text' },
    { name: 'email', label: 'Email', type: 'email' },
    { name: 'password', label: 'Password', type: 'password' },
    { name: 'confirmPassword', label: 'Confirm Password', type: 'password' },
  ];

  return (
    <div style={{ maxWidth: 440, margin: '60px auto', padding: 24, border: '1px solid #e2e8f0', borderRadius: 8 }}>
      <h2 style={{ textAlign: 'center', marginBottom: 24 }}>Create Account</h2>
      <form onSubmit={handleSubmit((d) => mutation.mutate(d))} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
        {fields.map(({ name, label, type }) => (
          <div key={name}>
            <label style={{ display: 'block', marginBottom: 4, fontWeight: 500 }}>{label}</label>
            <input {...register(name)} type={type} style={{ width: '100%', padding: '8px 12px', border: '1px solid #cbd5e0', borderRadius: 6, boxSizing: 'border-box' }} />
            {errors[name] && <p style={{ color: '#e53e3e', margin: '4px 0 0', fontSize: 13 }}>{errors[name]?.message}</p>}
          </div>
        ))}
        {mutation.isError && (
          <p style={{ color: '#e53e3e', textAlign: 'center', margin: 0 }}>
            {(mutation.error as any)?.response?.data?.message ?? 'Registration failed.'}
          </p>
        )}
        <button
          type="submit"
          disabled={mutation.isPending}
          style={{ background: '#3182ce', color: 'white', border: 'none', borderRadius: 6, padding: '10px', fontWeight: 600, cursor: 'pointer', fontSize: 16 }}
        >
          {mutation.isPending ? 'Creating account...' : 'Create Account'}
        </button>
      </form>
      <p style={{ textAlign: 'center', marginTop: 16 }}>
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
};
