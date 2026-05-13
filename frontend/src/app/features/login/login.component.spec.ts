import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideHttpClient(), provideRouter([])]
    }).compileComponents();
  });

  it('requires a valid email and password', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;

    component.form.setValue({ email: 'not-an-email', password: '' });

    expect(component.form.valid).toBe(false);
    expect(component.form.controls.email.hasError('email')).toBe(true);
    expect(component.form.controls.password.hasError('required')).toBe(true);
  });
});
