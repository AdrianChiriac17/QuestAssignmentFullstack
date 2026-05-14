import { computed, effect, Injectable, signal } from '@angular/core';
import { AddCartItemResult, CartItem } from '../models/cart-item.model';

export const CART_STORAGE_KEY = 'kitvault.cart';

export interface CartStockAdjustment {
  productId: string;
  size: string;
  availableQuantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly itemsSignal = signal<CartItem[]>(this.readItems());

  readonly items = this.itemsSignal.asReadonly();
  readonly itemCount = computed(() =>
    this.itemsSignal().reduce((count, item) => count + item.quantity, 0)
  );
  readonly totalPrice = computed(() =>
    this.itemsSignal().reduce((total, item) => total + item.unitPrice * item.quantity, 0)
  );

  constructor() {
    effect(() => {
      this.writeItems(this.itemsSignal());
    });
  }

  addItem(item: CartItem, maxStockQuantity: number): AddCartItemResult {
    if (item.quantity <= 0 || maxStockQuantity <= 0) {
      return {
        addedQuantity: 0,
        remainingQuantity: this.getRemainingQuantity(
          item.productId,
          item.selectedSize,
          maxStockQuantity)
      };
    }

    const existingQuantity = this.getQuantity(item.productId, item.selectedSize);
    const remainingQuantity = Math.max(maxStockQuantity - existingQuantity, 0);
    const addedQuantity = Math.min(item.quantity, remainingQuantity);

    if (addedQuantity === 0) {
      return {
        addedQuantity,
        remainingQuantity
      };
    }

    this.itemsSignal.update((items) => {
      const existingItem = items.find(
        (candidate) =>
          candidate.productId === item.productId &&
          candidate.selectedSize === item.selectedSize
      );

      if (existingItem === undefined) {
        return [...items, { ...item, quantity: addedQuantity }];
      }

      return items.map((candidate) =>
        candidate === existingItem
          ? { ...candidate, quantity: candidate.quantity + addedQuantity }
          : candidate
      );
    });

    return {
      addedQuantity,
      remainingQuantity: remainingQuantity - addedQuantity
    };
  }

  getQuantity(productId: string, selectedSize: string): number {
    return this.itemsSignal()
      .filter((item) => item.productId === productId && item.selectedSize === selectedSize)
      .reduce((quantity, item) => quantity + item.quantity, 0);
  }

  getRemainingQuantity(
    productId: string,
    selectedSize: string,
    maxStockQuantity: number
  ): number {
    return Math.max(maxStockQuantity - this.getQuantity(productId, selectedSize), 0);
  }

  updateQuantity(
    productId: string,
    selectedSize: string,
    quantity: number,
    maxStockQuantity: number
  ): void {
    const safeQuantity = Math.min(Math.max(quantity, 0), maxStockQuantity);

    if (safeQuantity === 0) {
      this.removeItem(productId, selectedSize);
      return;
    }

    this.itemsSignal.update((items) =>
      items.map((item) =>
        item.productId === productId && item.selectedSize === selectedSize
          ? { ...item, quantity: safeQuantity }
          : item
      )
    );
  }

  removeItem(productId: string, selectedSize: string): void {
    this.itemsSignal.update((items) =>
      items.filter((item) => item.productId !== productId || item.selectedSize !== selectedSize)
    );
  }

  applyStockAdjustments(adjustments: readonly CartStockAdjustment[]): void {
    if (adjustments.length === 0) {
      return;
    }

    this.itemsSignal.update((items) =>
      items.flatMap((item) => {
        const adjustment = adjustments.find(
          (candidate) =>
            candidate.productId === item.productId &&
            candidate.size === item.selectedSize);

        if (adjustment === undefined) {
          return [item];
        }

        if (adjustment.availableQuantity <= 0) {
          return [];
        }

        return [
          {
            ...item,
            quantity: Math.min(item.quantity, adjustment.availableQuantity)
          }
        ];
      })
    );
  }

  clear(): void {
    this.itemsSignal.set([]);
    localStorage.removeItem(CART_STORAGE_KEY);
  }

  private readItems(): CartItem[] {
    const storedItems = localStorage.getItem(CART_STORAGE_KEY);
    if (storedItems === null) {
      return [];
    }

    try {
      const parsedItems = JSON.parse(storedItems) as CartItem[];
      return Array.isArray(parsedItems) ? parsedItems : [];
    } catch {
      return [];
    }
  }

  private writeItems(items: CartItem[]): void {
    if (items.length === 0) {
      localStorage.removeItem(CART_STORAGE_KEY);
      return;
    }

    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
  }
}
