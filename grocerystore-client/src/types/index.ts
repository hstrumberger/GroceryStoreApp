export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string;
  user: User;
}

export interface ActiveSale {
  id: number;
  name: string;
  discountType: string;
  discountValue: number;
  endDate: string;
}

export interface ProductImage {
  id: number;
  url: string;
  altText?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  description?: string;
  price: number;
  discountedPrice?: number;
  categoryId: number;
  categoryName: string;
  manufacturerId?: number;
  manufacturerName?: string;
  averageRating: number;
  ratingCount: number;
  stockQuantity: number;
  isActive: boolean;
  images: ProductImage[];
  activeSale?: ActiveSale;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  description?: string;
  parentCategoryId?: number;
  displayOrder: number;
  subCategories: Category[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  unitPrice: number;
  discountedPrice?: number;
  quantity: number;
  lineTotal: number;
  imageUrl?: string;
  stockQuantity: number;
  activeSale?: ActiveSale;
}

export interface Cart {
  id: number;
  items: CartItem[];
  total: number;
}

export interface ShippingAddress {
  firstName: string;
  lastName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSku: string;
  unitPrice: number;
  discountedPrice: number;
  lineTotal: number;
  quantity: number;
}

export interface Order {
  id: number;
  status: string;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingAmount: number;
  total: number;
  shippingAddress: ShippingAddress;
  paymentMethod: string;
  paymentLast4?: string;
  createdAt: string;
  items: OrderItem[];
}

export interface OrderSummary {
  id: number;
  status: string;
  total: number;
  itemCount: number;
  createdAt: string;
}

export interface Sale {
  id: number;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  discountType: string;
  discountValue: number;
  isActive: boolean;
  productIds: number[];
  categoryIds: number[];
}
