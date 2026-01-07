import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiResponse, AnalyzeCvResponse, JobPosition } from '../models/job.model';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  analyzeCv(file: File): Observable<AnalyzeCvResponse> {
    const formData = new FormData();
    formData.append('cvFile', file);

    return this.http.post<ApiResponse<AnalyzeCvResponse>>(
      `${this.apiUrl}/Candidates/Analyze`,
      formData,
      {
        headers: new HttpHeaders({
          'x-api-version': environment.apiVersion
        })
      }
    ).pipe(
      map(response => response.data)
    );
  }

  getActiveJobs(companyCode?: string): Observable<JobPosition[]> {
    const url = companyCode 
      ? `${this.apiUrl}/jobs/active?companyCode=${companyCode}`
      : `${this.apiUrl}/jobs/active`;

    return this.http.get<ApiResponse<{ jobs: JobPosition[], totalCount: number }>>(
      url,
      {
        headers: new HttpHeaders({
          'x-api-version': environment.apiVersion
        })
      }
    ).pipe(
      map(response => response.data.jobs)
    );
  }

  syncJobs(): Observable<any> {
    return this.http.post<ApiResponse<any>>(
      `${this.apiUrl}/jobs/sync`,
      {},
      {
        headers: new HttpHeaders({
          'x-api-version': environment.apiVersion
        })
      }
    ).pipe(
      map(response => response.data)
    );
  }
}
