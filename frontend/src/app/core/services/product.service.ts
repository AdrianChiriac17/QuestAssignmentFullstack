import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { catchError, finalize, map, Observable, of, tap, throwError } from 'rxjs';
import { ProductResponseDto, ProductsResponseDto } from '../models/product.models';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class ProductService {
  
  private readonly http = inject(HttpClient);
  private readonly productsSignal = signal<ProductResponseDto[]>([]);
  private readonly isLoadingSignal = signal(false);
  private readonly errorMessageSignal = signal<string | null>(null);
  private readonly hasLoadedSignal = signal(false);

  readonly products = this.productsSignal.asReadonly();
  readonly isLoading = this.isLoadingSignal.asReadonly();
  readonly errorMessage = this.errorMessageSignal.asReadonly();
  readonly hasLoaded = this.hasLoadedSignal.asReadonly();

  loadProducts(): Observable<ProductResponseDto[]> {
    if (this.hasLoadedSignal()) {
      return of(this.productsSignal());
    }

    this.isLoadingSignal.set(true);
    this.errorMessageSignal.set(null);

    return this.http.get<ProductsResponseDto>(`${API_BASE_URL}/api/products`).pipe(
      map((response) => response.products),
      tap((products) => {
        this.productsSignal.set(products);
        this.hasLoadedSignal.set(true);
      }),
      catchError((error: HttpErrorResponse) => {
        this.errorMessageSignal.set(this.getErrorMessage(error));
        return throwError(() => error);
      }),
      finalize(() => {
        this.isLoadingSignal.set(false);
      })
    );
  }

  getProductById(id: string): ProductResponseDto | undefined {
    return this.productsSignal().find((product) => product.id === id);
  }

  getImageUrl(imageUrl: string | null | undefined): string {
    if (!imageUrl) {
      return '';
    }

    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }

    return `${API_BASE_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    return error.error?.message ?? error.error?.detail ?? error.error?.title ?? 'Unable to load products.';
  }
}
