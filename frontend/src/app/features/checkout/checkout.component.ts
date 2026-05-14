import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiErrorResponseDto } from '../../core/models/api-error-response.model';
import {
  CheckoutRequestDto,
  CheckoutResponseDto,
  CheckoutStockConflictItemResponseDto,
  CheckoutStockConflictResponseDto
} from '../../core/models/checkout.models';
import { ProductSize } from '../../core/models/product.models';
import { CartService } from '../../core/services/cart.service';
import { CheckoutService } from '../../core/services/checkout.service';
import { ProductService } from '../../core/services/product.service';
import { OrderConfirmationComponent } from './order-confirmation/order-confirmation.component';

@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, OrderConfirmationComponent, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly checkoutService = inject(CheckoutService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly productService = inject(ProductService);

  protected readonly items = this.cartService.items;
  protected readonly itemCount = this.cartService.itemCount;
  protected readonly totalPrice = this.cartService.totalPrice;
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly order = signal<CheckoutResponseDto | null>(null);
  protected readonly stockConflicts = signal<CheckoutStockConflictItemResponseDto[]>([]);

  protected readonly form = this.formBuilder.nonNullable.group({
    recipientName: ['', [Validators.required, Validators.maxLength(150)]],
    addressLine: ['', [Validators.required, Validators.maxLength(255)]],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    postalCode: ['', [Validators.required, Validators.maxLength(20)]],
    country: ['', [Validators.required, Validators.maxLength(100)]]
  });

  ngOnInit(): void {
    this.productService.loadProducts().subscribe({
      error: () => {
        // Checkout can still submit using cart snapshot data; backend validates stock and prices.
      }
    });
  }

  protected placeOrder(): void {
    this.errorMessage.set(null);
    this.stockConflicts.set([]);

    if (this.items().length === 0) {
      this.errorMessage.set('Your cart is empty.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.checkoutService.placeOrder(this.toCheckoutRequest()).subscribe({
      next: (response) => {
        this.order.set(response);
        this.cartService.clear();
        this.productService.refreshProducts().subscribe({
          error: () => {
            // The order is already placed; the catalog can recover on the next load.
          }
        });
        this.isSubmitting.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.handleCheckoutError(error);
        this.isSubmitting.set(false);
      }
    });
  }

  protected getConflictProductName(conflict: CheckoutStockConflictItemResponseDto): string {
    return this.items().find((item) => item.productId === conflict.productId)?.productName
      ?? this.productService.getProductById(conflict.productId)?.name
      ?? 'Selected product';
  }

  private toCheckoutRequest(): CheckoutRequestDto {
    const address = this.form.getRawValue();

    return {
      items: this.items().map((item) => ({
        productId: item.productId,
        size: item.selectedSize as ProductSize,
        quantity: item.quantity
      })),
      recipientName: address.recipientName,
      addressLine: address.addressLine,
      city: address.city,
      postalCode: address.postalCode,
      country: address.country
    };
  }

  private handleCheckoutError(error: HttpErrorResponse): void {
    if (error.status === 409) {
      const conflictResponse = error.error as CheckoutStockConflictResponseDto;
      const conflicts = conflictResponse.errors ?? [];
      this.stockConflicts.set(conflicts);
      this.cartService.applyStockAdjustments(conflicts);
      this.productService.refreshProducts().subscribe({
        error: () => {
          // The stock-conflict response still lets the cart recover if refresh fails.
        }
      });
      this.errorMessage.set(
        conflicts.length > 0
          ? 'Some cart quantities were updated because stock changed.'
          : conflictResponse.message ?? 'Some items are no longer available.'
      );
      return;
    }

    const response = error.error as ApiErrorResponseDto | undefined;
    const validationMessage = response?.errors
      ? Object.values(response.errors).flat().at(0)
      : undefined;

    this.errorMessage.set(
      response?.message ??
      validationMessage ??
      response?.detail ??
      response?.title ??
      'Unable to place order.'
    );
  }
}
