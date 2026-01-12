export interface JobPosition {
  publicId?: string;
  jobPositionId?: number;
  title: string;
  externalUrl: string;
  companyCode: string;
  jobCode: string;
  summary?: string;
  responsibilities?: string;
  skills?: string;
  mustHaveSkills?: string;
  niceToHaveSkills?: string;
  isActive?: boolean;
  closedAt?: Date;
  lastCheckedAt?: Date;
  createdAt?: Date;
  lastUpdated?: Date;
}

export interface PagedList<T> {
  items: T[];
  count: number;
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
  referralReason?: string;
  whatIsMissingForPerfectMatch?: string;
}

export interface CandidateInfo {
  candidateId: string;
  name?: string;
  email?: string;
  hasResume: boolean;
}

export interface AnalyzeCvResponse {
  totalPositionsAnalyzed: number;
  matchesFound: number;
  matches: JobMatch[];
  candidateInfo?: CandidateInfo;
}

export interface ApiResponse<T> {
  message: string;
  data: T;
}
