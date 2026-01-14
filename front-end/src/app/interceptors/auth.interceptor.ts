import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();

    if (token) {
      request = this.addToken(request, token);
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Don't retry 401 errors from the refresh token endpoint to avoid infinite loops
        if (error.status === 401 && !request.url.includes('/Auth/RefreshToken')) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private addToken(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.authService.refreshToken().pipe(
        switchMap((response) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(response.token);
          return next.handle(this.addToken(request, response.token));
        }),
        catchError((error) => {
          this.isRefreshing = false;
          // Emit empty string to signal failure to queued requests
          this.refreshTokenSubject.next('');
          this.authService.logout();
          return throwError(() => error);
        })
      );
    } else {
      // Queue requests while token is being refreshed
      return this.refreshTokenSubject.pipe(
        filter((token): token is string => token !== null),
        take(1),
        switchMap(token => {
          if (token !== '') {
            return next.handle(this.addToken(request, token));
          }
          // Token is empty string, which means refresh failed
          return throwError(() => new HttpErrorResponse({ 
            error: 'Token refresh failed', 
            status: 401,
            statusText: 'Unauthorized'
          }));
        })
      );
    }
  }
}
