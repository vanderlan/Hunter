export interface HiringProcess {
  publicId: string;
  status: string; // Backend sends status as string (e.g., "Referred", "Screening")
  candidate: CandidateInfo;
  jobPosition: JobPositionInfo;
  createdByUser: UserInfo;
  lastUpdatedByUser?: UserInfo;
  statusHistory: StatusHistoryItem[];
  notes: NoteItem[];
  referralReason?: string;
  lastAIAnalysis?: LastAIAnalysisInfo;
  createdAt: Date;
  lastUpdated: Date;
}

export interface HiringProcessListItem {
  publicId: string;
  status: string;
  candidateName: string;
  candidateEmail: string;
  jobTitle: string;
  createdByUserName: string;
  createdAt: Date;
  lastUpdated: Date;
}

export interface CandidateInfo {
  publicId: string;
  name?: string;
  email?: string;
  phoneNumber?: string;
  hasResume?: boolean;
}

export interface JobPositionInfo {
  publicId: string;
  title: string;
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

export interface LastAIAnalysisInfo {
  cvAnalysisPublicId: string;
  analyzedAt: Date;
  matchPercentage: number;
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
  New = 0,
  PendingAnalysis = 1,
  ReferredInternally = 2,
  Screening = 3,
  ConsultantTechnicalInterview = 4,
  ClientInterview = 5,
  Hired = 6,
  Reproved = 7
}

// Centralized configuration for all hiring process statuses
export interface HiringProcessStatusConfig {
  value: HiringProcessStatus;
  backendKey: string; // What backend sends/expects
  label: string; // User-friendly display name
  cssClass: string; // CSS class for badge styling
  materialColor: 'primary' | 'accent' | 'warn' | ''; // Material theme color
  priority: number; // For sorting
}

export const HIRING_PROCESS_STATUS_CONFIG: Record<string, HiringProcessStatusConfig> = {
  'New': {
    value: HiringProcessStatus.New,
    backendKey: 'New',
    label: 'New',
    cssClass: 'status-new',
    materialColor: 'accent',
    priority: 1
  },
  'PendingAnalysis': {
    value: HiringProcessStatus.PendingAnalysis,
    backendKey: 'PendingAnalysis',
    label: 'Pending Analysis',
    cssClass: 'status-referred-partner',
    materialColor: 'accent',
    priority: 2
  },
  'ReferredInternally': {
    value: HiringProcessStatus.ReferredInternally,
    backendKey: 'ReferredInternally',
    label: 'Referred Internally',
    cssClass: 'status-referred-internal',
    materialColor: 'accent',
    priority: 2
  },
  'Screening': {
    value: HiringProcessStatus.Screening,
    backendKey: 'Screening',
    label: 'Screening',
    cssClass: 'status-screening',
    materialColor: 'accent',
    priority: 3
  },
  'ConsultantTechnicalInterview': {
    value: HiringProcessStatus.ConsultantTechnicalInterview,
    backendKey: 'ConsultantTechnicalInterview',
    label: 'Technical Interview',
    cssClass: 'status-technical',
    materialColor: 'accent',
    priority: 4
  },
  'ClientInterview': {
    value: HiringProcessStatus.ClientInterview,
    backendKey: 'ClientInterview',
    label: 'Client Interview',
    cssClass: 'status-client',
    materialColor: 'accent',
    priority: 5
  },
  'Hired': {
    value: HiringProcessStatus.Hired,
    backendKey: 'Hired',
    label: 'Hired',
    cssClass: 'status-hired',
    materialColor: 'primary',
    priority: 6
  },
  'Reproved': {
    value: HiringProcessStatus.Reproved,
    backendKey: 'Reproved',
    label: 'Reproved',
    cssClass: 'status-reproved',
    materialColor: 'warn',
    priority: 7
  }
};

// Helper functions to access status configuration
export class HiringProcessStatusHelper {
  static getConfig(status: string): HiringProcessStatusConfig | undefined {
    return HIRING_PROCESS_STATUS_CONFIG[status];
  }

  static getLabel(status: string): string {
    return this.getConfig(status)?.label || status;
  }

  static getCssClass(status: string): string {
    return this.getConfig(status)?.cssClass || '';
  }

  static getMaterialColor(status: string): 'primary' | 'accent' | 'warn' | '' {
    return this.getConfig(status)?.materialColor || '';
  }

  static getPriority(status: string): number {
    return this.getConfig(status)?.priority || 999;
  }

  static getEnumValue(status: string): HiringProcessStatus {
    return this.getConfig(status)?.value || HiringProcessStatus.New;
  }

  static getAllStatusOptions(): Array<{ value: HiringProcessStatus; label: string }> {
    return Object.values(HIRING_PROCESS_STATUS_CONFIG).map(config => ({
      value: config.value,
      label: config.label
    }));
  }
}

export interface StartHiringProcessRequest {
  candidateId: string;
  jobPositionId: string;
  referralReason?: string;
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

export interface GenerateAIInsightsResponse {
  referralReason: string;
}
