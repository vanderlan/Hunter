import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { JobPosition, PagedList } from '../models/job.model';

@Injectable({
  providedIn: 'root'
})
export class JobPositionService {
  private readonly apiUrl = `${environment.apiUrl}/JobPositions`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getJobPositions(page: number = 1, quantity: number = 10, query?: string): Observable<PagedList<JobPosition>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', quantity.toString());
    
    if (query) {
      params = params.set('filter.Query', query);
    }

    return this.http.get<PagedList<JobPosition>>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  getJobPositionById(id: string): Observable<JobPosition> {
    return this.http.get<JobPosition>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  createJobPosition(jobPosition: JobPosition): Observable<JobPosition> {
    return this.http.post<JobPosition>(this.apiUrl, jobPosition, {
      headers: this.getHeaders()
    });
  }

  updateJobPosition(id: string, jobPosition: JobPosition): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, jobPosition, {
      headers: this.getHeaders()
    });
  }

  deleteJobPosition(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}
