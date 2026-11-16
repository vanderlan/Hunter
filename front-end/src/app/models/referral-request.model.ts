export type ReferralRequestStatus = 'Pending' | 'Accepted' | 'Declined';

export interface ReferralRequest {
  publicId: string;
  status: ReferralRequestStatus;
  candidateId: string;
  candidateName?: string;
  candidateEmail?: string;
  candidatePhoneNumber?: string;
  jobPositionId: string;
  jobTitle: string;
  matchPercentage?: number;
  message?: string;
  createdAt: string;
  resolvedAt?: string;
  resolvedByUserName?: string;
  resolutionNote?: string;
  hiringProcessId?: string;
}

export interface PagedReferralRequests {
  items: ReferralRequest[];
  count: number;
}

export interface ReferralRequestAcceptResponse {
  referralRequestId: string;
  hiringProcessId: string;
  status: string;
}
