import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedReferralRequests, ReferralRequestAcceptResponse } from '../models/referral-request.model';

@Injectable({
  providedIn: 'root'
})
export class ReferralRequestService {
  private readonly apiUrl = `${environment.apiUrl}/ReferralRequests`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getReferralRequests(page = 1, quantity = 10, status?: number, query?: string): Observable<PagedReferralRequests> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', quantity.toString());

    if (status !== undefined && status !== null) {
      params = params.set('status', status.toString());
    }
    if (query) {
      params = params.set('query', query);
    }

    return this.http.get<PagedReferralRequests>(this.apiUrl, { headers: this.getHeaders(), params });
  }

  getPendingCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/PendingCount`, { headers: this.getHeaders() });
  }

  accept(publicId: string, note?: string): Observable<ReferralRequestAcceptResponse> {
    return this.http.post<ReferralRequestAcceptResponse>(`${this.apiUrl}/${publicId}/Accept`, { note }, { headers: this.getHeaders() });
  }

  decline(publicId: string, note?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${publicId}/Decline`, { note }, { headers: this.getHeaders() });
  }
}
