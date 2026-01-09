import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Candidate, PagedCandidates, ResumeUploadResponse } from '../models/candidate.model';

@Injectable({
  providedIn: 'root'
})
export class CandidateService {
  private readonly apiUrl = `${environment.apiUrl}/Candidates`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getCandidates(page: number = 1, quantity: number = 10, query?: string): Observable<PagedCandidates> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', quantity.toString());
    
    if (query) {
      params = params.set('query', query);
    }

    return this.http.get<PagedCandidates>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  getCandidateById(id: string): Observable<Candidate> {
    return this.http.get<Candidate>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  updateCandidate(id: string, candidate: Candidate): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, candidate, {
      headers: this.getHeaders()
    });
  }

  uploadResume(candidateId: string, file: File): Observable<ResumeUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ResumeUploadResponse>(
      `${this.apiUrl}/${candidateId}/Resume`, 
      formData, 
      {
        headers: this.getHeaders()
      }
    );
  }

  getResumeUrl(candidateId: string): string {
    return `${this.apiUrl}/${candidateId}/Resume`;
  }

  downloadResume(candidateId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${candidateId}/Resume`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    });
  }

  hasResume(candidateId: string): Observable<boolean> {
    return new Observable(observer => {
      this.http.head(`${this.apiUrl}/${candidateId}/Resume`, {
        headers: this.getHeaders(),
        observe: 'response'
      }).subscribe({
        next: () => observer.next(true),
        error: () => observer.next(false),
        complete: () => observer.complete()
      });
    });
  }
}
