import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PRODUCT_SIZES, ProductSize } from '../../core/models/product.models';
import { CartService } from '../../core/services/cart.service';
import { ProductService } from '../../core/services/product.service';
import { CartFeedbackQueueComponent } from '../../shared/cart-feedback-queue/cart-feedback-queue.component';

@Component({
  selector: 'app-product-details',
  imports: [CartFeedbackQueueComponent, CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css'
})
export class ProductDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly cartService = inject(CartService);
  private readonly productService = inject(ProductService);
  private readonly cartFeedbackQueue = viewChild.required(CartFeedbackQueueComponent);

  protected readonly productId = signal<string | null>(null);
  protected readonly selectedSize = signal<ProductSize>('M');
  protected readonly quantity = signal(1);
  protected readonly productSizes = PRODUCT_SIZES;
  protected readonly isLoading = this.productService.isLoading;
  protected readonly errorMessage = this.productService.errorMessage;
  protected readonly hasLoaded = this.productService.hasLoaded;

  protected readonly product = computed(() => {
    const id = this.productId();
    return id === null ? undefined : this.productService.getProductById(id);
  });

  protected readonly frontImageUrl = computed(() =>
    this.productService.getImageUrl(this.product()?.frontImageUrl)
  );

  protected readonly backImageUrl = computed(() =>
    this.productService.getImageUrl(this.product()?.backImageUrl)
  );

  protected readonly selectedStock = computed(() => {
    return this.product()?.sizes.find((stock) => stock.size === this.selectedSize());
  });

  protected readonly stockQuantity = computed(() => this.selectedStock()?.stockQuantity ?? 0);
  
  protected readonly quantityInCart = computed(() => {
    const product = this.product();
    if (product === undefined) {
      return 0;
    }

    return this.cartService.getQuantity(product.id, this.selectedSize());
  });
  
  protected readonly cartSummary = computed(() => {
    const product = this.product();
    if (product === undefined) {
      return 'You do not have this shirt in your cart yet.';
    }

    const selectedCartQuantities = this.productSizes
      .map((size) => ({
        size,
        quantity: this.cartService.getQuantity(product.id, size)
      }))
      .filter((item) => item.quantity > 0)
      .map((item) => `${item.quantity} ${item.size}`);

    if (selectedCartQuantities.length === 0) {
      return 'You do not have this shirt in your cart yet.';
    }

    return `You already have ${selectedCartQuantities.join(', ')} in your cart.`;
  });
  protected readonly maxQuantity = computed(() => {
    const product = this.product();
    if (product === undefined) {
      return 0;
    }

    return this.cartService.getRemainingQuantity(
      product.id,
      this.selectedSize(),
      this.stockQuantity());
  });

  protected readonly canDecreaseQuantity = computed(() => this.quantity() > this.minimumQuantity());
  protected readonly canIncreaseQuantity = computed(() => this.quantity() < this.maxQuantity());
  protected readonly canAddToCart = computed(() => this.maxQuantity() > 0 && this.quantity() > 0);

  ngOnInit(): void {
    this.productId.set(this.route.snapshot.paramMap.get('id'));

    this.productService.loadProducts().subscribe({
      next: () => {
        this.initializeSelection();
      },
      error: () => {
        // The service owns the user-facing error signal.
      }
    });
  }

  protected selectSize(size: ProductSize): void {
    this.selectedSize.set(size);
    this.quantity.set(this.maxQuantity() > 0 ? 1 : 0);
  }

  protected updateQuantity(value: string): void {
    const parsedValue = Number(value);
    if (!Number.isInteger(parsedValue)) {
      return;
    }

    this.quantity.set(Math.min(Math.max(parsedValue, 1), this.maxQuantity()));
  }

  protected decreaseQuantity(): void {
    if (!this.canDecreaseQuantity()) {
      return;
    }

    this.quantity.update((quantity) => quantity - 1);
  }

  protected increaseQuantity(): void {
    if (!this.canIncreaseQuantity()) {
      return;
    }

    this.quantity.update((quantity) => quantity + 1);
  }

  protected addToCart(): void {
    const product = this.product();
    if (product === undefined || !this.canAddToCart()) {
      return;
    }

    const result = this.cartService.addItem(
      {
        productId: product.id,
        productName: product.name,
        selectedSize: this.selectedSize(),
        unitPrice: product.price,
        quantity: this.quantity(),
        imageUrl: product.frontImageUrl
      },
      this.stockQuantity());

    if (result.addedQuantity === 0) {
      this.cartFeedbackQueue().enqueue(
        `All available ${this.selectedSize()} shirts are already in your cart.`);
      return;
    }

    this.quantity.set(this.maxQuantity() > 0 ? 1 : 0);
    this.cartFeedbackQueue().enqueue(`Added ${result.addedQuantity} ${this.selectedSize()} to your cart.`);
  }

  protected getStockQuantity(size: ProductSize): number {
    return this.product()?.sizes.find((stock) => stock.size === size)?.stockQuantity ?? 0;
  }

  protected minimumQuantity(): number {
    return this.maxQuantity() > 0 ? 1 : 0;
  }

  private initializeSelection(): void {
    const product = this.product();
    if (product === undefined) {
      return;
    }

    const firstAvailableSize = product.sizes.find((stock) => stock.stockQuantity > 0);
    const fallbackSize = product.sizes[0]?.size ?? 'M';
    this.selectedSize.set(firstAvailableSize?.size ?? fallbackSize);
    const remainingQuantity = this.maxQuantity();
    this.quantity.set(remainingQuantity > 0 ? 1 : 0);
  }
}
