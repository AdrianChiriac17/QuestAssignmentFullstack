import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AUTH_SESSION_STORAGE_KEY, AuthService } from './auth.service';
import { API_BASE_URL } from './api.config';
import { CART_STORAGE_KEY, CartService } from './cart.service';

describe('AuthService', () => {
  let authService: AuthService;
  let cartService: CartService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });

    authService = TestBed.inject(AuthService);
    cartService = TestBed.inject(CartService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
    localStorage.clear();
  });

  it('persists the login session after a successful login', () => {
    authService.login({ email: 'user@example.com', password: 'Password1' }).subscribe();

    const request = httpTestingController.expectOne(`${API_BASE_URL}/api/Auth/login`);
    request.flush({
      token: 'jwt-token',
      user: {
        id: '6ed61585-0f8c-4023-bf6a-6936ecff6777',
        email: 'user@example.com',
        firstName: 'Ada',
        lastName: 'Lovelace'
      }
    });

    expect(authService.token()).toBe('jwt-token');
    expect(authService.user()?.email).toBe('user@example.com');
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toContain('jwt-token');
  });

  it('clears auth and cart state on logout', () => {
    cartService.addItem({
      productId: 'shirt-1',
      productName: 'Home Classic',
      selectedSize: 'M',
      unitPrice: 79,
      quantity: 2
    });

    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cartService.items()));
    authService.logout();

    expect(authService.session()).toBeNull();
    expect(cartService.items()).toEqual([]);
    expect(localStorage.getItem(AUTH_SESSION_STORAGE_KEY)).toBeNull();
    expect(localStorage.getItem(CART_STORAGE_KEY)).toBeNull();
  });
});
