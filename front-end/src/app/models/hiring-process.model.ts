export interface HiringProcess {
  publicId: string;
  status: HiringProcessStatus;
  candidate: CandidateInfo;
  jobPosition: JobPositionInfo;
  createdByUser: UserInfo;
  lastUpdatedByUser?: UserInfo;
  statusHistory: StatusHistoryItem[];
  notes: NoteItem[];
  createdAt: Date;
  lastUpdated: Date;
}

export interface HiringProcessListItem {
  publicId: string;
  status: string;
  candidateName: string;
  candidateEmail: string;
  jobTitle: string;
  jobCode: string;
  companyCode: string;
  createdByUserName: string;
  createdAt: Date;
  lastUpdated: Date;
}

export interface CandidateInfo {
  publicId: string;
  name?: string;
  email?: string;
}

export interface JobPositionInfo {
  publicId: string;
  title: string;
  companyCode: string;
  jobCode: string;
}

export interface UserInfo {
  name: string;
  email: string;
}

export interface StatusHistoryItem {
  fromStatus: string;
  toStatus: string;
  changedByUserName: string;
  notes?: string;
  createdAt: Date;
}

export interface NoteItem {
  publicId: string;
  note: string;
  userName: string;
  createdAt: Date;
}

export interface DashboardStats {
  statusCounts: { [key: string]: number };
  totalProcesses: number;
  activeProcesses: number;
  hiredCount: number;
  reprovedCount: number;
}

export interface PagedHiringProcesses {
  items: HiringProcessListItem[];
  count: number;
}

export enum HiringProcessStatus {
  Referred = 'Referred',
  Screening = 'Screening',
  ConsultantTechnicalInterview = 'ConsultantTechnicalInterview',
  ClientInterview = 'ClientInterview',
  Hired = 'Hired',
  Reproved = 'Reproved'
}

export interface StartHiringProcessRequest {
  candidateId: string;
  jobPositionId: string;
}

export interface StartHiringProcessResponse {
  publicId: string;
  status: string;
}

export interface UpdateStatusRequest {
  newStatus: HiringProcessStatus;
  notes?: string;
}

export interface AddNoteRequest {
  note: string;
}
