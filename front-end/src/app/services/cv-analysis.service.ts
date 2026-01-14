import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CvAnalysisHistoryItem {
  analysisId: string;
  candidateName: string;
  candidateEmail: string;
  analyzedAt: Date;
  analyzedByUserName: string;
  totalPositionsAnalyzed: number;
  matchesFound: number;
}

export interface PagedAnalysisHistory {
  items: CvAnalysisHistoryItem[];
  count: number;
}

export interface CvAnalysisDetail {
  analysisId: string;
  candidateInfo: {
    candidateId: string;
    name?: string;
    email?: string;
    hasResume: boolean;
  };
  analyzedAt: Date;
  analyzedByUserName: string;
  totalPositionsAnalyzed: number;
  matchesFound: number;
  matches: any[];
}

@Injectable({
  providedIn: 'root'
})
export class CvAnalysisService {
  private readonly apiUrl = `${environment.apiUrl}/CvAnalysis`;

  constructor(private http: HttpClient) {}

  getHistory(page: number = 1, pageSize: number = 10): Observable<PagedAnalysisHistory> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('quantity', pageSize.toString());

    return this.http.get<PagedAnalysisHistory>(`${this.apiUrl}/History`, {
      params,
      headers: new HttpHeaders({
        'x-api-version': environment.apiVersion
      })
    });
  }

  getAnalysisDetail(analysisId: string): Observable<CvAnalysisDetail> {
    return this.http.get<CvAnalysisDetail>(`${this.apiUrl}/${analysisId}`, {
      headers: new HttpHeaders({
        'x-api-version': environment.apiVersion
      })
    });
  }
}
