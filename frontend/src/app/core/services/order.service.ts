import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { OrderResponseDto, OrdersResponseDto } from '../models/order.models';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  loadOrders(): Observable<OrdersResponseDto> {
    return this.http.get<OrdersResponseDto>(`${API_BASE_URL}/api/orders`);
  }

  loadOrderById(orderId: string): Observable<OrderResponseDto> {
    return this.http.get<OrderResponseDto>(`${API_BASE_URL}/api/orders/${orderId}`);
  }
}
