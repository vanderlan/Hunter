import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { HiringProcessService } from '../../services/hiring-process.service';
import { CvAnalysisService } from '../../services/cv-analysis.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { HiringProcess, HiringProcessStatus } from '../../models/hiring-process.model';

@Component({
    selector: 'app-hiring-process-detail',
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatChipsModule,
        MatDividerModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatSnackBarModule,
        MatDialogModule,
        FormsModule
    ],
    templateUrl: './hiring-process-detail.component.html',
    styleUrl: './hiring-process-detail.component.scss'
})
export class HiringProcessDetailComponent implements OnInit {
  process: HiringProcess | null = null;
  loading = true;
  updatingStatus = false;
  addingNote = false;
  newNote = '';
  selectedStatus: HiringProcessStatus | null = null;
  statusNotes = '';
  isAdmin = false;
  generatingInsights = false;

  allStatusOptions = [
    { value: HiringProcessStatus.New, label: 'New' },
    { value: HiringProcessStatus.ReferredByPartner, label: 'Referred by Partner' },
    { value: HiringProcessStatus.ReferredInternally, label: 'Referred Internally' },
    { value: HiringProcessStatus.Screening, label: 'Screening' },
    { value: HiringProcessStatus.ConsultantTechnicalInterview, label: 'Technical Interview' },
    { value: HiringProcessStatus.ClientInterview, label: 'Client Interview' },
    { value: HiringProcessStatus.Hired, label: 'Hired' },
    { value: HiringProcessStatus.Reproved, label: 'Reproved' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiringProcessService: HiringProcessService,
    private cvAnalysisService: CvAnalysisService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProcess(id);
    }
  }

  loadProcess(id: string): void {
    this.loading = true;
    this.hiringProcessService.getHiringProcessById(id).subscribe({
      next: (data) => {
        this.process = data;
        // Map string status from API to numeric enum for the select dropdown
        this.selectedStatus = this.mapStringToEnum(data.status);
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading hiring process');
        console.error('Error loading process:', error);
        this.router.navigate(['/hiring-process']);
      }
    });
  }

  mapStringToEnum(status: string): HiringProcessStatus {
    switch (status) {
      case 'New':
        return HiringProcessStatus.New;
      case 'ReferredByPartner':
        return HiringProcessStatus.ReferredByPartner;
      case 'ReferredInternally':
        return HiringProcessStatus.ReferredInternally;
      case 'Screening':
        return HiringProcessStatus.Screening;
      case 'ConsultantTechnicalInterview':
        return HiringProcessStatus.ConsultantTechnicalInterview;
      case 'ClientInterview':
        return HiringProcessStatus.ClientInterview;
      case 'Hired':
        return HiringProcessStatus.Hired;
      case 'Reproved':
        return HiringProcessStatus.Reproved;
      default:
        return HiringProcessStatus.New;
    }
  }

  updateStatus(): void {
    if (!this.process || !this.selectedStatus || !this.isAdmin) {
      return;
    }

    const currentStatusEnum = this.mapStringToEnum(this.process.status);
    if (this.selectedStatus === currentStatusEnum) {
      this.notificationService.warning('Please select a different status');
      return;
    }

    this.updatingStatus = true;
    this.hiringProcessService.updateStatus(this.process.publicId, {
      newStatus: this.selectedStatus,
      notes: this.statusNotes || undefined
    }).subscribe({
      next: () => {
        this.updatingStatus = false;
        this.notificationService.success('Status updated successfully');
        this.statusNotes = '';
        this.loadProcess(this.process!.publicId);
      },
      error: (error) => {
        this.updatingStatus = false;
        this.notificationService.error('Error updating status');
        console.error('Error updating status:', error);
      }
    });
  }

  addNote(): void {
    if (!this.process || !this.newNote.trim()) {
      return;
    }

    this.addingNote = true;
    this.hiringProcessService.addNote(this.process.publicId, {
      note: this.newNote.trim()
    }).subscribe({
      next: () => {
        this.addingNote = false;
        this.notificationService.success('Note added successfully');
        this.newNote = '';
        this.loadProcess(this.process!.publicId);
      },
      error: (error) => {
        this.addingNote = false;
        this.notificationService.error('Error adding note');
        console.error('Error adding note:', error);
      }
    });
  }

  generateAIInsights(): void {
    if (!this.process || !this.isAdmin) {
      return;
    }

    this.generatingInsights = true;
    this.hiringProcessService.generateAIInsights(this.process.publicId).subscribe({
      next: (response) => {
        this.generatingInsights = false;
        this.notificationService.success('AI insights generated successfully!');
        // Update the local process object with the new referral reason
        if (this.process) {
          this.process.referralReason = response.referralReason;
        }
      },
      error: (error) => {
        this.generatingInsights = false;
        const errorMessage = error.error?.message || 'Error generating AI insights. Please try again.';
        this.notificationService.error(errorMessage);
        console.error('Error generating AI insights:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/hiring-process']);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Hired':
        return 'primary';
      case 'Reproved':
        return 'warn';
      case 'New':
      case 'ReferredByPartner':
      case 'ReferredInternally':
      case 'Screening':
      case 'ConsultantTechnicalInterview':
      case 'ClientInterview':
        return 'accent';
      default:
        return '';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'ReferredByPartner':
        return 'Referred by Partner';
      case 'ReferredInternally':
        return 'Referred Internally';
      case 'ConsultantTechnicalInterview':
        return 'Technical Interview';
      case 'ClientInterview':
        return 'Client Interview';
      default:
        return status;
    }
  }

  get availableStatusOptions() {
    if (!this.process) {
      return this.allStatusOptions;
    }
    
    const currentStatusEnum = this.mapStringToEnum(this.process.status);
    return this.allStatusOptions.filter(option => option.value !== currentStatusEnum);
  }

  viewResume(): void {
    if (!this.process || !this.process.candidate.publicId) {
      return;
    }

    import('../resume-viewer/resume-viewer-dialog.component').then(m => {
      this.dialog.open(m.ResumeViewerDialogComponent, {
        data: {
          candidateId: this.process!.candidate.publicId,
          candidateName: this.process!.candidate.name || 'Candidate'
        },
        width: '90vw',
        height: '90vh',
        maxWidth: '1200px',
        panelClass: 'resume-viewer-dialog'
      });
    });
  }

  viewAIAnalysis(cvAnalysisPublicId: string): void {
    // Load the analysis detail and show in a dialog
    this.cvAnalysisService.getAnalysisDetail(cvAnalysisPublicId).subscribe({
      next: (analysisDetail) => {
        import('./ai-analysis-dialog.component').then(m => {
          this.dialog.open(m.AIAnalysisDialogComponent, {
            data: analysisDetail,
            width: '90vw',
            height: '90vh',
            maxWidth: '1400px',
            panelClass: 'ai-analysis-dialog'
          });
        });
      },
      error: (error) => {
        this.notificationService.error('Error loading AI analysis');
        console.error('Error loading AI analysis:', error);
      }
    });
  }

  cancelReferral(): void {
    if (!this.process) {
      return;
    }

    import('./cancel-confirmation-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CancelConfirmationDialogComponent, {
        data: {
          candidateName: this.process!.candidate.name || 'Unknown',
          jobTitle: this.process!.jobPosition.title
        },
        width: '550px',
        maxWidth: '90vw',
        disableClose: true
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result?.confirmed && result?.reason) {
          this.performCancellation(result.reason);
        }
      });
    });
  }

  private performCancellation(reason: string): void {
    if (!this.process) {
      return;
    }

    this.loading = true;
    this.hiringProcessService.cancelHiringProcess(this.process.publicId, reason).subscribe({
      next: () => {
        this.notificationService.success('Referral cancelled successfully');
        this.router.navigate(['/hiring-process']);
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error cancelling referral');
        console.error('Error cancelling referral:', error);
      }
    });
  }
}
