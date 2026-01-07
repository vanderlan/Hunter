export interface Candidate {
  publicId: string;
  name?: string;
  email?: string;
  phoneNumber?: string;
  analyzedAt: Date;
  analyzedByUserName: string;
  createdAt: Date;
}

export interface PagedCandidates {
  items: Candidate[];
  count: number;
}
