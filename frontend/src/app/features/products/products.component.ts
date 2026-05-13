import { Component, inject, OnInit } from '@angular/core';
import { ProductService } from '../../core/services/product.service';
import { ProductCardComponent } from '../../shared/product-card/product-card.component';

@Component({
  selector: 'app-products',
  imports: [ProductCardComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  private readonly productService = inject(ProductService);

  protected readonly products = this.productService.products;
  protected readonly isLoading = this.productService.isLoading;
  protected readonly errorMessage = this.productService.errorMessage;

  ngOnInit(): void {
    this.productService.loadProducts().subscribe({
      error: () => {
        // The service owns the user-facing error signal.
      }
    });
  }
}
