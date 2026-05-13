import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [provideHttpClient(), provideRouter([])]
    }).compileComponents();
  });

  it('matches backend registration validation rules', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;

    component.form.setValue({
      firstName: '',
      lastName: 'Smith',
      email: 'member@example.com',
      password: 'password'
    });

    expect(component.form.valid).toBe(false);
    expect(component.form.controls.firstName.hasError('required')).toBe(true);
    expect(component.form.controls.password.hasError('pattern')).toBe(true);
  });
});
