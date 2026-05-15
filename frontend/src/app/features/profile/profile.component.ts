import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderSummaryResponseDto } from '../../core/models/order.models';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { OrderCardComponent } from './order-card/order-card.component';
import { ProfileSummaryComponent } from './profile-summary/profile-summary.component';

@Component({
  selector: 'app-profile',
  imports: [OrderCardComponent, ProfileSummaryComponent, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly orderService = inject(OrderService);

  protected readonly user = this.authService.user;
  protected readonly orders = signal<OrderSummaryResponseDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly lifetimeSpend = computed(() =>
    this.orders().reduce((total, order) => total + order.totalPrice, 0)
  );

  ngOnInit(): void {
    this.loadOrders();
  }

  protected loadOrders(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.orderService.loadOrders().subscribe({
      next: (response) => {
        this.orders.set(response.orders);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load your orders.');
        this.isLoading.set(false);
      }
    });
  }

}
