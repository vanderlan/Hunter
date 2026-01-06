import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { User, UserCreateRequest, UserUpdateRequest, PagedUsers } from '../models/user.model';

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  newPasswordConfirm: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/Users`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getUsers(page: number = 1, quantity: number = 10, query?: string): Observable<PagedUsers> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', quantity.toString());
    
    if (query) {
      params = params.set('query', query);
    }

    return this.http.get<PagedUsers>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  createUser(user: UserCreateRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, user, {
      headers: this.getHeaders()
    });
  }

  updateUser(id: string, user: UserUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, user, {
      headers: this.getHeaders()
    });
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/Me/Password`, request, {
      headers: this.getHeaders()
    });
  }
}
