import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { AUTH_SESSION_STORAGE_KEY } from './core/services/auth.service';

describe('App', () => {
  afterEach(() => {
    localStorage.clear();
  });

  async function createApp() {
    TestBed.resetTestingModule();

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideRouter([])]
    }).compileComponents();

    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();

    return fixture.nativeElement as HTMLElement;
  }

  it('shows public navigation links when logged out', async () => {
    const compiled = await createApp();
    const navText = compiled.querySelector('header')?.textContent ?? '';

    expect(navText).toContain('Products');
    expect(navText).toContain('Login');
    expect(navText).not.toContain('Cart');
    expect(navText).not.toContain('Profile');
    expect(navText).not.toContain('Logout');
  });

  it('shows account navigation links when logged in', async () => {
    localStorage.setItem(
      AUTH_SESSION_STORAGE_KEY,
      JSON.stringify({
        token: 'jwt-token',
        user: {
          id: '6ed61585-0f8c-4023-bf6a-6936ecff6777',
          email: 'user@example.com',
          firstName: 'Ada',
          lastName: 'Lovelace'
        }
      })
    );

    const compiled = await createApp();
    const navText = compiled.querySelector('header')?.textContent ?? '';

    expect(navText).toContain('Products');
    expect(navText).toContain('Cart');
    expect(navText).toContain('Profile');
    expect(navText).toContain('Logout');
    expect(navText).not.toContain('Login');
  });
});
