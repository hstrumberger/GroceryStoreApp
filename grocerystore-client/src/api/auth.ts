import apiClient from './client';
import type { AuthResponse, User } from '../types';

export const register = (data: { firstName: string; lastName: string; email: string; password: string }) =>
  apiClient.post<AuthResponse>('/auth/register', data).then((r) => r.data);

export const login = (data: { email: string; password: string }) =>
  apiClient.post<AuthResponse>('/auth/login', data).then((r) => r.data);

export const refresh = () =>
  apiClient.post<AuthResponse>('/auth/refresh').then((r) => r.data);

export const logout = () =>
  apiClient.post('/auth/logout');

export const getMe = () =>
  apiClient.get<User>('/auth/me').then((r) => r.data);
