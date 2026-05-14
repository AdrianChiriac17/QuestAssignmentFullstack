import { ProductSize } from './product.models';

export interface OrderSummaryResponseDto {
  id: string;
  totalPrice: number;
  createdAt: string;
}

export interface OrdersResponseDto {
  orders: OrderSummaryResponseDto[];
}

export interface OrderItemResponseDto {
  productId: string;
  productName: string;
  size: ProductSize;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderResponseDto {
  id: string;
  recipientName: string;
  addressLine: string;
  city: string;
  postalCode: string;
  country: string;
  totalPrice: number;
  createdAt: string;
  items: OrderItemResponseDto[];
}
