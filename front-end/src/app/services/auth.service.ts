import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RefreshTokenRequest } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'hunter_token';
  private readonly REFRESH_TOKEN_KEY = 'hunter_refresh_token';
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.isAuthenticated());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    if (typeof window !== 'undefined') {
      this.checkTokenExpiration();
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials, {
      headers: new HttpHeaders({
        'x-api-version': environment.apiVersion
      })
    }).pipe(
      tap(response => {
        this.setTokens(response.token, response.refreshToken);
        this.isAuthenticatedSubject.next(true);
      })
    );
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh-token`, request, {
      headers: new HttpHeaders({
        'x-api-version': environment.apiVersion
      })
    }).pipe(
      tap(response => {
        this.setTokens(response.token, response.refreshToken);
      })
    );
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    }
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return this.hasToken() && !this.isTokenExpired();
  }

  getUserProfile(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Try the full claim URI first, then fallback to short form
      return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
          || payload.role 
          || null;
    } catch {
      return null;
    }
  }

  getUserName(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Use the correct claim URI for givenname
      return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] 
          || payload.name 
          || payload.givenname
          || null;
    } catch {
      return null;
    }
  }

  getUserEmail(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Use the correct claim URI for email address
      return payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] 
          || payload.email 
          || payload.emailaddress
          || null;
    } catch {
      return null;
    }
  }

  isAdmin(): boolean {
    const profile = this.getUserProfile();
    return profile === 'admin';
  }

  private setTokens(token: string, refreshToken: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.TOKEN_KEY, token);
      localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    }
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }

  private isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiration = payload.exp * 1000;
      return Date.now() >= expiration;
    } catch {
      return true;
    }
  }

  private checkTokenExpiration(): void {
    setInterval(() => {
      if (this.hasToken() && this.isTokenExpired()) {
        this.logout();
      }
    }, 60000);
  }
}
