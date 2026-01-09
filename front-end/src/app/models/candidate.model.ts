export interface Candidate {
  publicId: string;
  name?: string;
  email?: string;
  phoneNumber?: string;
  analyzedAt: Date;
  analyzedByUserName: string;
  createdAt: Date;
  hasResume?: boolean;
  resumeFileName?: string;
}

export interface PagedCandidates {
  items: Candidate[];
  count: number;
}

export interface ResumeUploadResponse {
  fileId: string;
  fileName: string;
  fileSize: number;
}
