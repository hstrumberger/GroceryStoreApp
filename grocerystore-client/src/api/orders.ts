import apiClient from './client';
import type { Order, OrderSummary, ShippingAddress } from '../types';

export interface CheckoutPayload {
  shippingAddress: ShippingAddress;
  payment: {
    method: string;
    cardNumber: string;
    cardHolderName: string;
    expiryDate: string;
    cvv: string;
  };
}

export const checkout = (data: CheckoutPayload) =>
  apiClient.post<Order>('/checkout', data).then((r) => r.data);

export const getOrders = () =>
  apiClient.get<OrderSummary[]>('/orders').then((r) => r.data);

export const getOrder = (id: number) =>
  apiClient.get<Order>(`/orders/${id}`).then((r) => r.data);
