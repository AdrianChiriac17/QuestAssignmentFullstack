export const PRODUCT_SIZES = ['XS', 'S', 'M', 'L', 'XL', 'XXL'] as const;

export type ProductSize = (typeof PRODUCT_SIZES)[number];

export interface ProductSizeStockResponseDto {
  size: ProductSize;
  stockQuantity: number;
}

export interface ProductResponseDto {
  id: string;
  name: string;
  description: string;
  price: number;
  frontImageUrl: string;
  backImageUrl?: string | null;
  sizes: ProductSizeStockResponseDto[];
}

export interface ProductsResponseDto {
  products: ProductResponseDto[];
}
