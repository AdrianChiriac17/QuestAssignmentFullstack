import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { CartComponent } from './features/cart/cart.component';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { LandingComponent } from './features/landing/landing.component';
import { LoginComponent } from './features/login/login.component';
import { ProductDetailsComponent } from './features/product-details/product-details.component';
import { ProductsComponent } from './features/products/products.component';
import { ProfileComponent } from './features/profile/profile.component';
import { RegisterComponent } from './features/register/register.component';

export const routes: Routes = [
  { path: '', component: LandingComponent, title: 'KitVault' },
  { path: 'products', component: ProductsComponent, title: 'Products | KitVault' },
  {
    path: 'products/:id',
    component: ProductDetailsComponent,
    canActivate: [authGuard],
    title: 'Product Details | KitVault'
  },
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard],
    title: 'Login | KitVault'
  },
  {
    path: 'register',
    component: RegisterComponent,
    canActivate: [guestGuard],
    title: 'Register | KitVault'
  },
  {
    path: 'cart',
    component: CartComponent,
    canActivate: [authGuard],
    title: 'Cart | KitVault'
  },
  {
    path: 'checkout',
    component: CheckoutComponent,
    canActivate: [authGuard],
    title: 'Checkout | KitVault'
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [authGuard],
    title: 'Profile | KitVault'
  },
  { path: '**', redirectTo: '' }
];
