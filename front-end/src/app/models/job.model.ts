export interface JobPosition {
  publicId: string;
  title: string;
  externalUrl: string;
  companyCode: string;
  jobCode: string;
  summary?: string;
  mustHaveSkills?: string;
  niceToHaveSkills?: string;
  createdAt: Date;
  lastCheckedAt: Date;
}

export interface JobMatch {
  jobCode: string;
  jobTitle: string;
  companyCode: string;
  externalUrl: string;
  matchPercentage: number;
  matchReason: string;
  matchingSkills: string[];
  missingSkills: string[];
  summary?: string;
  mustHaveSkills?: string;
  niceToHaveSkills?: string;
}

export interface AnalyzeCvResponse {
  totalPositionsAnalyzed: number;
  matchesFound: number;
  matches: JobMatch[];
}

export interface ApiResponse<T> {
  message: string;
  data: T;
}
