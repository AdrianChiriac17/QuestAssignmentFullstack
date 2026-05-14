import { CurrencyPipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { CartItem } from '../../../core/models/cart-item.model';

@Component({
  selector: 'app-cart-item-row',
  imports: [CurrencyPipe],
  templateUrl: './cart-item-row.component.html',
  styleUrl: './cart-item-row.component.css'
})
export class CartItemRowComponent {
  readonly item = input.required<CartItem>();
  readonly maxQuantity = input.required<number>();
  readonly imageUrl = input.required<string>();
  readonly quantityChange = output<number>();
  readonly remove = output<void>();

  protected readonly lineTotal = computed(() => this.item().unitPrice * this.item().quantity);
  protected readonly canDecreaseQuantity = computed(() => this.item().quantity > 1);
  protected readonly canIncreaseQuantity = computed(() => this.item().quantity < this.maxQuantity());

  protected decreaseQuantity(): void {
    if (!this.canDecreaseQuantity()) {
      return;
    }

    this.quantityChange.emit(this.item().quantity - 1);
  }

  protected increaseQuantity(): void {
    if (!this.canIncreaseQuantity()) {
      return;
    }

    this.quantityChange.emit(this.item().quantity + 1);
  }

  protected updateQuantity(value: string): void {
    const parsedValue = Number(value);
    if (!Number.isInteger(parsedValue)) {
      return;
    }

    this.quantityChange.emit(Math.min(Math.max(parsedValue, 1), this.maxQuantity()));
  }
}
