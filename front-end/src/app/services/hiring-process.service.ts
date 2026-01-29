import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  HiringProcess,
  PagedHiringProcesses,
  DashboardStats,
  StartHiringProcessRequest,
  StartHiringProcessResponse,
  UpdateStatusRequest,
  AddNoteRequest,
  GenerateAIInsightsResponse
} from '../models/hiring-process.model';

@Injectable({
  providedIn: 'root'
})
export class HiringProcessService {
  private readonly apiUrl = `${environment.apiUrl}/HiringProcess`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'x-api-version': environment.apiVersion
    });
  }

  getHiringProcesses(
    page: number = 1, 
    quantity: number = 10, 
    candidateId?: number,
    jobPositionId?: number,
    status?: number,
    query?: string
  ): Observable<PagedHiringProcesses> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', quantity.toString());
    
    if (candidateId) {
      params = params.set('candidateId', candidateId.toString());
    }
    if (jobPositionId) {
      params = params.set('jobPositionId', jobPositionId.toString());
    }
    if (status !== undefined) {
      params = params.set('status', status.toString());
    }
    if (query) {
      params = params.set('query', query);
    }

    return this.http.get<PagedHiringProcesses>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  getHiringProcessById(id: string): Observable<HiringProcess> {
    return this.http.get<HiringProcess>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  getDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/Dashboard`, {
      headers: this.getHeaders()
    });
  }

  startHiringProcess(request: StartHiringProcessRequest): Observable<StartHiringProcessResponse> {
    return this.http.post<StartHiringProcessResponse>(this.apiUrl, request, {
      headers: this.getHeaders()
    });
  }

  updateStatus(id: string, request: UpdateStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/Status`, request, {
      headers: this.getHeaders()
    });
  }

  addNote(id: string, request: AddNoteRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/Notes`, request, {
      headers: this.getHeaders()
    });
  }

  generateAIInsights(id: string): Observable<GenerateAIInsightsResponse> {
    return this.http.post<GenerateAIInsightsResponse>(`${this.apiUrl}/${id}/GenerateAIInsights`, {}, {
      headers: this.getHeaders()
    });
  }

  cancelHiringProcess(id: string, cancelReason: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders(),
      body: { cancelReason }
    });
  }
}
