import { ProductSize } from './product.models';

export interface CheckoutItemRequestDto {
  productId: string;
  size: ProductSize;
  quantity: number;
}

export interface CheckoutRequestDto {
  items: CheckoutItemRequestDto[];
  recipientName: string;
  addressLine: string;
  city: string;
  postalCode: string;
  country: string;
}

export interface CheckoutResponseDto {
  orderId: string;
  totalPrice: number;
  createdAt: string;
}

export interface CheckoutStockConflictItemResponseDto {
  productId: string;
  size: ProductSize;
  requestedQuantity: number;
  availableQuantity: number;
}

export interface CheckoutStockConflictResponseDto {
  message: string;
  errors: CheckoutStockConflictItemResponseDto[];
}
