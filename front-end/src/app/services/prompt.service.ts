import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Prompt {
  publicId: string;
  key: string;
  name: string;
  text: string;
}

export interface PromptUpdateRequest {
  name?: string;
  text?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PromptService {
  private readonly apiUrl = `${environment.apiUrl}/Prompts`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getAll(): Observable<Prompt[]> {
    return this.http.get<Prompt[]>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  getByKey(key: string): Observable<Prompt> {
    return this.http.get<Prompt>(`${this.apiUrl}/key/${key}`, {
      headers: this.getHeaders()
    });
  }

  update(id: string, request: PromptUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request, {
      headers: this.getHeaders()
    });
  }
}
