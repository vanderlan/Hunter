/**
 * Models for the anonymous (public) surface of Hunter.
 * Nothing here carries internal ids: positions are addressed by `code`,
 * the visitor by an opaque `candidateToken`.
 */
export interface PublicJobPosition {
  code: string;
  title: string;
  summary?: string;
  mustHaveSkills?: string;
  niceToHaveSkills?: string;
  publishedAt: string;
}

export interface PublicJobPositionsResponse {
  jobs: PublicJobPosition[];
  totalCount: number;
}

export interface PublicJobPositionDetail extends PublicJobPosition {
  responsibilities?: string;
  requirements?: string;
}

export interface PublicJobMatch {
  jobCode: string;
  jobTitle: string;
  matchPercentage: number;
  matchReason: string;
  matchingSkills: string[];
  missingSkills: string[];
  whatIsMissingForPerfectMatch?: string;
  canRequestReferral: boolean;
}

export interface PublicCvMatchResponse {
  candidateToken?: string;
  totalPositionsAnalyzed: number;
  matchesFound: number;
  extractedName?: string;
  extractedEmail?: string;
  extractedPhone?: string;
  minimumMatchForReferral: number;
  matches: PublicJobMatch[];
}

export interface RegisterPublicCandidateRequest {
  name: string;
  email: string;
  phoneNumber?: string;
  receiveJobMatchNotifications: boolean;
  hasAcceptedTerms: boolean;
}

export interface RegisterPublicCandidateResponse {
  email: string;
  isEmailConfirmed: boolean;
  isConfirmationEmailSent: boolean;
}

export interface ConfirmEmailResponse {
  name?: string;
  email: string;
  isAlreadyConfirmed: boolean;
  pendingReferralRequests: number;
}

export interface CreateReferralRequest {
  jobCode: string;
  message?: string;
}

export interface CreateReferralResponse {
  jobTitle: string;
  status: string;
  isEmailConfirmationRequired: boolean;
}

/** Client-side session for the public CV match flow (kept in sessionStorage). */
export interface PublicMatchSession {
  result: PublicCvMatchResponse;
  profile?: RegisterPublicCandidateRequest;
  isEmailConfirmed: boolean;
  requestedJobCodes: string[];
}
