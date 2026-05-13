import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly cartService = inject(CartService);

  protected readonly isLoggedIn = this.authService.isLoggedIn;
  protected readonly cartCount = this.cartService.itemCount;

  protected logout(): void {
    this.authService.logout();
  }
}
