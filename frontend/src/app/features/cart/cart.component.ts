import { Component, inject, OnInit, signal } from '@angular/core';
import { CartItem } from '../../core/models/cart-item.model';
import { CartService } from '../../core/services/cart.service';
import { ProductService } from '../../core/services/product.service';
import { CartItemRowComponent } from './cart-item-row/cart-item-row.component';
import { CartSummaryComponent } from './cart-summary/cart-summary.component';

@Component({
  selector: 'app-cart',
  imports: [CartItemRowComponent, CartSummaryComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly productService = inject(ProductService);

  protected readonly items = this.cartService.items;
  protected readonly itemCount = this.cartService.itemCount;
  protected readonly totalPrice = this.cartService.totalPrice;
  protected readonly checkoutMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.productService.loadProducts().subscribe({
      error: () => {
        // Cart rows can still render from persisted cart data if products fail to load.
      }
    });
  }

  protected clearCart(): void {
    this.checkoutMessage.set(null);
    this.cartService.clear();
  }

  protected checkout(): void {
    this.checkoutMessage.set('Checkout will be added in the next vertical slice.');
  }

  protected updateQuantity(item: CartItem, quantity: number): void {
    this.cartService.updateQuantity(
      item.productId,
      item.selectedSize,
      quantity,
      this.getMaxQuantity(item));
  }

  protected removeItem(item: CartItem): void {
    this.cartService.removeItem(item.productId, item.selectedSize);
  }

  protected getImageUrl(item: CartItem): string {
    return this.productService.getImageUrl(item.imageUrl);
  }

  protected getMaxQuantity(item: CartItem): number {
    const product = this.productService.getProductById(item.productId);
    const sizeStock = product?.sizes.find((stock) => stock.size === item.selectedSize);

    return sizeStock?.stockQuantity ?? item.quantity;
  }
}
