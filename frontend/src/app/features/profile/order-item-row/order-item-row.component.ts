import { CurrencyPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { OrderItemResponseDto } from '../../../core/models/order.models';

@Component({
  selector: 'app-order-item-row',
  imports: [CurrencyPipe],
  templateUrl: './order-item-row.component.html',
  styleUrl: './order-item-row.component.css'
})
export class OrderItemRowComponent {
  readonly item = input.required<OrderItemResponseDto>();
}
