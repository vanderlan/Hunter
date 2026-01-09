import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Candidate, PagedCandidates } from '../models/candidate.model';

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
}
