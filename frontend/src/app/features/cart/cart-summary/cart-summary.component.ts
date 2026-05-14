import { CurrencyPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-cart-summary',
  imports: [CurrencyPipe],
  templateUrl: './cart-summary.component.html',
  styleUrl: './cart-summary.component.css'
})
export class CartSummaryComponent {
  readonly itemCount = input.required<number>();
  readonly totalPrice = input.required<number>();
  readonly canCheckout = input.required<boolean>();
  readonly checkout = output<void>();

  protected checkoutClicked(): void {
    if (!this.canCheckout()) {
      return;
    }

    this.checkout.emit();
  }
}
