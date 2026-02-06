import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CredentialStatusResponse {
  hasActiveCredential: boolean;
  credentialId?: string;
  expiresAt?: string;
  isExpired: boolean;
  lastSyncedAt?: string;
  daysUntilExpiration?: number;
  updatedByUserId: string;
  createdAt?: string;
  message: string;
}

export interface StoreCredentialsResponse {
  credentialId: string;
  expiresAt: string;
  isValid: boolean;
  message: string;
}

export interface SyncPrioritiesResponse {
  totalPrioritiesInCsv: number;
  matchedJobs: number;
  unmatchedJobs: number;
  updatedJobs: number;
  unmatchedJobCodes: string[];
  updatedJobCodes: string[];
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class LookerStudioService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getCredentialStatus(): Observable<CredentialStatusResponse> {
    return this.http.get<CredentialStatusResponse>(
      `${this.apiUrl}/LookerStudio/credentials/status`,
      { headers: this.getHeaders() }
    );
  }

  storeCredentials(curlCommand: string, expiresInDays: number = 7): Observable<StoreCredentialsResponse> {
    return this.http.post<StoreCredentialsResponse>(
      `${this.apiUrl}/LookerStudio/credentials`,
      { curlCommand, expiresInDays },
      { headers: this.getHeaders() }
    );
  }

  syncPriorities(): Observable<SyncPrioritiesResponse> {
    return this.http.post<SyncPrioritiesResponse>(
      `${this.apiUrl}/LookerStudio/sync-priorities`,
      {},
      { headers: this.getHeaders() }
    );
  }
}
