import apiClient from './client';
import type { Category, PagedResult, Product } from '../types';

export const getProducts = (params?: {
  categoryId?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
}) => apiClient.get<PagedResult<Product>>('/products', { params }).then((r) => r.data);

export const getProduct = (id: number) =>
  apiClient.get<Product>(`/products/${id}`).then((r) => r.data);

export const getCategories = () =>
  apiClient.get<Category[]>('/categories').then((r) => r.data);

export const getProductsByCategory = (slug: string, params?: { page?: number; pageSize?: number }) =>
  apiClient.get<PagedResult<Product>>(`/categories/${slug}/products`, { params }).then((r) => r.data);
