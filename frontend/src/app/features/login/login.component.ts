import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiErrorResponseDto } from '../../core/models/api-error-response.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    password: ['', [Validators.required, Validators.maxLength(100)]]
  });

  submit(): void {
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        void this.router.navigateByUrl(this.getReturnUrl());
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isSubmitting.set(false);
      }
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    const response = error.error as ApiErrorResponseDto | undefined;
    return response?.message ?? response?.detail ?? response?.title ?? 'Unable to log in.';
  }

  private getReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    if (returnUrl !== null && returnUrl.startsWith('/') && !returnUrl.startsWith('//')) {
      return returnUrl;
    }

    return '/products';
  }
}
