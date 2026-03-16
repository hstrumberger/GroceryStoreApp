import apiClient from './client';
import type { Cart } from '../types';

export const getCart = () =>
  apiClient.get<Cart>('/cart').then((r) => r.data);

export const addToCart = (productId: number, quantity: number) =>
  apiClient.post<Cart>('/cart/items', { productId, quantity }).then((r) => r.data);

export const updateCartItem = (productId: number, quantity: number) =>
  apiClient.put<Cart>(`/cart/items/${productId}`, { quantity }).then((r) => r.data);

export const removeCartItem = (productId: number) =>
  apiClient.delete<Cart>(`/cart/items/${productId}`).then((r) => r.data);

export const clearCart = () =>
  apiClient.delete('/cart');
