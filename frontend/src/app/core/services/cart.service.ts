import { computed, effect, Injectable, signal } from '@angular/core';
import { CartItem } from '../models/cart-item.model';

export const CART_STORAGE_KEY = 'kitvault.cart';

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

  addItem(item: CartItem): void {
    this.itemsSignal.update((items) => {
      const existingItem = items.find(
        (candidate) =>
          candidate.productId === item.productId &&
          candidate.selectedSize === item.selectedSize
      );

      if (existingItem === undefined) {
        return [...items, item];
      }

      return items.map((candidate) =>
        candidate === existingItem
          ? { ...candidate, quantity: candidate.quantity + item.quantity }
          : candidate
      );
    });
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
