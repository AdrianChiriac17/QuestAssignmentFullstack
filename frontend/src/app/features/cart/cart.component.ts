import { Component, inject, signal } from '@angular/core';
import { CartService } from '../../core/services/cart.service';
import { CartSummaryComponent } from './cart-summary/cart-summary.component';

@Component({
  selector: 'app-cart',
  imports: [CartSummaryComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {
  private readonly cartService = inject(CartService);

  protected readonly items = this.cartService.items;
  protected readonly itemCount = this.cartService.itemCount;
  protected readonly totalPrice = this.cartService.totalPrice;
  protected readonly checkoutMessage = signal<string | null>(null);

  protected clearCart(): void {
    this.checkoutMessage.set(null);
    this.cartService.clear();
  }

  protected checkout(): void {
    this.checkoutMessage.set('Checkout will be added in the next vertical slice.');
  }
}
