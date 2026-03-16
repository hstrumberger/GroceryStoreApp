import apiClient from './client';
import type { Sale } from '../types';

export const getActiveSales = () =>
  apiClient.get<Sale[]>('/sales/active').then((r) => r.data);

export const getSale = (id: number) =>
  apiClient.get<Sale>(`/sales/${id}`).then((r) => r.data);

export const search = (params: {
  q?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}) => apiClient.get('/search', { params }).then((r) => r.data);
