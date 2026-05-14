import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CheckoutRequestDto, CheckoutResponseDto } from '../models/checkout.models';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class CheckoutService {
  private readonly http = inject(HttpClient);

  placeOrder(request: CheckoutRequestDto): Observable<CheckoutResponseDto> {
    return this.http.post<CheckoutResponseDto>(`${API_BASE_URL}/api/checkout`, request);
  }
}
