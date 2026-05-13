import { Component, computed, inject, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductResponseDto } from '../../core/models/product.models';
import { AuthService } from '../../core/services/auth.service';
import { ProductService } from '../../core/services/product.service';

@Component({
  selector: 'app-product-card',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  private readonly authService = inject(AuthService);
  private readonly productService = inject(ProductService);

  readonly product = input.required<ProductResponseDto>();

  protected readonly detailLink = computed(() =>
    this.authService.isLoggedIn() ? ['/products', this.product().id] : ['/login']
  );

  protected readonly frontImageUrl = computed(() =>
    this.productService.getImageUrl(this.product().frontImageUrl)
  );
  
  protected readonly backImageUrl = computed(() =>
    this.productService.getImageUrl(this.product().backImageUrl)
  );
}
