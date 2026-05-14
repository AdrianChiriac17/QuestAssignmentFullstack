import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, input, signal } from '@angular/core';
import { OrderResponseDto, OrderSummaryResponseDto } from '../../../core/models/order.models';
import { OrderService } from '../../../core/services/order.service';
import { OrderItemRowComponent } from '../order-item-row/order-item-row.component';

@Component({
  selector: 'app-order-card',
  imports: [CurrencyPipe, DatePipe, OrderItemRowComponent],
  templateUrl: './order-card.component.html',
  styleUrl: './order-card.component.css'
})
export class OrderCardComponent {
  private readonly orderService = inject(OrderService);

  readonly order = input.required<OrderSummaryResponseDto>();

  protected readonly isExpanded = signal(false);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly orderDetails = signal<OrderResponseDto | null>(null);

  protected toggleDetails(): void {
    if (this.isExpanded()) {
      this.isExpanded.set(false);
      return;
    }

    this.isExpanded.set(true);

    if (this.orderDetails() !== null || this.isLoading()) {
      return;
    }

    this.loadDetails();
  }

  protected retryDetails(): void {
    this.loadDetails();
  }

  protected getShortOrderId(): string {
    const id = this.order().id;
    return `${id.slice(0, 8)}-${id.slice(-4)}`.toUpperCase();
  }

  private loadDetails(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.orderService.loadOrderById(this.order().id).subscribe({
      next: (response) => {
        this.orderDetails.set(response);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load order details.');
        this.isLoading.set(false);
      }
    });
  }
}
