export interface CartItem {
  productId: string;
  productName: string;
  selectedSize: string;
  unitPrice: number;
  quantity: number;
  imageUrl?: string;
}

export interface AddCartItemResult {
  addedQuantity: number;
  remainingQuantity: number;
}
