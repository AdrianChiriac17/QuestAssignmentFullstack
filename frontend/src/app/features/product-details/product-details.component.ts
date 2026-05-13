import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PRODUCT_SIZES, ProductSize } from '../../core/models/product.models';
import { ProductService } from '../../core/services/product.service';

@Component({
  selector: 'app-product-details',
  imports: [CurrencyPipe, FormsModule, RouterLink],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css'
})
export class ProductDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);

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

  protected readonly maxQuantity = computed(() => this.selectedStock()?.stockQuantity ?? 0);

  protected readonly canAddToCart = computed(() => this.maxQuantity() > 0 && this.quantity() > 0);
  
  protected readonly quantityOptions = computed(() =>
    Array.from({ length: this.maxQuantity() }, (_, index) => index + 1)
  );

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

  protected addToCart(): void {
    if (!this.canAddToCart()) {
      return;
    }

    void this.router.navigateByUrl('/products');
  }

  protected getStockQuantity(size: ProductSize): number {
    return this.product()?.sizes.find((stock) => stock.size === size)?.stockQuantity ?? 0;
  }

  private initializeSelection(): void {
    const product = this.product();
    if (product === undefined) {
      return;
    }

    const firstAvailableSize = product.sizes.find((stock) => stock.stockQuantity > 0);
    const fallbackSize = product.sizes[0]?.size ?? 'M';
    this.selectedSize.set(firstAvailableSize?.size ?? fallbackSize);
    this.quantity.set((firstAvailableSize?.stockQuantity ?? 0) > 0 ? 1 : 0);
  }
}
