import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Parameter {
  publicId: string;
  key: string;
  value: string;
}

export interface ParameterUpdateRequest {
  value: string;
}

@Injectable({
  providedIn: 'root'
})
export class ParameterService {
  private readonly apiUrl = `${environment.apiUrl}/Parameters`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getAll(): Observable<Parameter[]> {
    return this.http.get<Parameter[]>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  getByKey(key: string): Observable<Parameter> {
    return this.http.get<Parameter>(`${this.apiUrl}/key/${key}`, {
      headers: this.getHeaders()
    });
  }

  update(id: string, request: ParameterUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request, {
      headers: this.getHeaders()
    });
  }
}
