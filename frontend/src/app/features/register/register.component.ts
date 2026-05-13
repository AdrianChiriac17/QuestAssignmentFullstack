import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiErrorResponseDto } from '../../core/models/api-error-response.model';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(20),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)
      ]
    ]
  });

  submit(): void {
    this.errorMessage.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.authService.register(this.form.getRawValue()).subscribe({
      next: () => {
        void this.router.navigateByUrl('/login');
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.getErrorMessage(error));
        this.isSubmitting.set(false);
      }
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    const response = error.error as ApiErrorResponseDto | undefined;
    const validationMessage = response?.errors
      ? Object.values(response.errors).flat().at(0)
      : undefined;

    return (
      response?.message ??
      validationMessage ??
      response?.detail ??
      response?.title ??
      'Unable to register.'
    );
  }
}
