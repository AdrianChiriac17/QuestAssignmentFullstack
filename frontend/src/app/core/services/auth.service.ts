import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  AuthSession,
  LoginUserRequestDto,
  LoginUserResponseDto,
  RegisterUserRequestDto,
  UserResponseDto
} from '../models/auth.models';
import { API_BASE_URL } from './api.config';
import { CartService } from './cart.service';

export const AUTH_SESSION_STORAGE_KEY = 'kitvault.authSession';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly cartService = inject(CartService);
  private readonly sessionSignal = signal<AuthSession | null>(this.readSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly token = computed(() => this.sessionSignal()?.token ?? null);
  readonly user = computed(() => this.sessionSignal()?.user ?? null);
  readonly isLoggedIn = computed(() => this.token() !== null);

  login(request: LoginUserRequestDto): Observable<LoginUserResponseDto> {
    return this.http
      .post<LoginUserResponseDto>(`${API_BASE_URL}/api/Auth/login`, request)
      .pipe(
        tap((response) => {
          const currentUserId = this.user()?.id;
          if (currentUserId !== undefined && currentUserId !== response.user.id) {
            this.cartService.clear();
          }

          this.setSession(response);
        })
      );
  }

  register(request: RegisterUserRequestDto): Observable<UserResponseDto> {
    return this.http.post<UserResponseDto>(`${API_BASE_URL}/api/Auth/register`, request);
  }

  logout(): void {
    this.sessionSignal.set(null);
    localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
    this.cartService.clear();
  }

  private setSession(session: AuthSession): void {
    this.sessionSignal.set(session);
    localStorage.setItem(AUTH_SESSION_STORAGE_KEY, JSON.stringify(session));
  }

  private readSession(): AuthSession | null {
    const storedSession = localStorage.getItem(AUTH_SESSION_STORAGE_KEY);
    if (storedSession === null) {
      return null;
    }

    try {
      const parsedSession = JSON.parse(storedSession) as AuthSession;
      if (parsedSession.token && parsedSession.user) {
        return parsedSession;
      }
    } catch {
      localStorage.removeItem(AUTH_SESSION_STORAGE_KEY);
    }

    return null;
  }
}
