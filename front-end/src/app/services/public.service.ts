import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ConfirmEmailResponse,
  CreateReferralRequest,
  CreateReferralResponse,
  PublicCvMatchResponse,
  PublicJobPositionDetail,
  PublicJobPositionsResponse,
  PublicMatchSession,
  RegisterPublicCandidateRequest,
  RegisterPublicCandidateResponse
} from '../models/public.model';

/**
 * Calls the anonymous `/Public` endpoints and keeps the visitor's CV match
 * session in sessionStorage so a refresh does not lose the results.
 */
@Injectable({
  providedIn: 'root'
})
export class PublicService {
  private readonly apiUrl = `${environment.apiUrl}/Public`;
  private readonly sessionKey = 'hunter_public_match_session';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getJobPositions(): Observable<PublicJobPositionsResponse> {
    return this.http.get<PublicJobPositionsResponse>(`${this.apiUrl}/JobPositions`, { headers: this.getHeaders() });
  }

  getJobPosition(code: string): Observable<PublicJobPositionDetail> {
    return this.http.get<PublicJobPositionDetail>(`${this.apiUrl}/JobPositions/${encodeURIComponent(code)}`, { headers: this.getHeaders() });
  }

  matchCv(file: File): Observable<PublicCvMatchResponse> {
    const formData = new FormData();
    formData.append('cvFile', file);
    return this.http.post<PublicCvMatchResponse>(`${this.apiUrl}/CvMatch`, formData, { headers: this.getHeaders() });
  }

  register(candidateToken: string, request: RegisterPublicCandidateRequest): Observable<RegisterPublicCandidateResponse> {
    return this.http.post<RegisterPublicCandidateResponse>(
      `${this.apiUrl}/Candidates/${encodeURIComponent(candidateToken)}/Register`,
      request,
      { headers: this.getHeaders() }
    );
  }

  confirmEmail(token: string): Observable<ConfirmEmailResponse> {
    return this.http.post<ConfirmEmailResponse>(`${this.apiUrl}/Candidates/Confirm`, { token }, { headers: this.getHeaders() });
  }

  requestReferral(candidateToken: string, request: CreateReferralRequest): Observable<CreateReferralResponse> {
    return this.http.post<CreateReferralResponse>(
      `${this.apiUrl}/Candidates/${encodeURIComponent(candidateToken)}/ReferralRequests`,
      request,
      { headers: this.getHeaders() }
    );
  }

  // ---- session persistence -------------------------------------------------

  loadSession(): PublicMatchSession | null {
    if (typeof sessionStorage === 'undefined') {
      return null;
    }
    const raw = sessionStorage.getItem(this.sessionKey);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as PublicMatchSession;
    } catch {
      sessionStorage.removeItem(this.sessionKey);
      return null;
    }
  }

  saveSession(session: PublicMatchSession): void {
    if (typeof sessionStorage !== 'undefined') {
      sessionStorage.setItem(this.sessionKey, JSON.stringify(session));
    }
  }

  clearSession(): void {
    if (typeof sessionStorage !== 'undefined') {
      sessionStorage.removeItem(this.sessionKey);
    }
  }
}
