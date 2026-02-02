import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { NgxFileDropModule, NgxFileDropEntry, FileSystemFileEntry } from 'ngx-file-drop';
import { JobService } from '../../services/job.service';
import { HiringProcessService } from '../../services/hiring-process.service';
import { CvAnalysisService, CvAnalysisHistoryItem } from '../../services/cv-analysis.service';
import { NotificationService } from '../../services/notification.service';
import { JobMatch, CandidateInfo } from '../../models/job.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
    selector: 'app-cv-analyzer',
    imports: [
        CommonModule,
        FormsModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatProgressBarModule,
        MatChipsModule,
        MatExpansionModule,
        MatSnackBarModule,
        MatDialogModule,
        MatDividerModule,
        MatListModule,
        MatFormFieldModule,
        MatInputModule,
        NgxFileDropModule
    ],
    templateUrl: './cv-analyzer.component.html',
    styleUrl: './cv-analyzer.component.scss'
})
export class CvAnalyzerComponent implements OnInit {
selectedFile: File | null = null;
analyzing = false;
matches: JobMatch[] = [];
totalPositions = 0;
dragOver = false;
candidateInfo: CandidateInfo | null = null;

// Analysis history section
analysisHistory: CvAnalysisHistoryItem[] = [];
loadingHistory = false;
historyPage = 1;
historyPageSize = 4;
historyTotalCount = 0;
searchQuery = '';
private searchSubject = new Subject<string>();

constructor(
  private jobService: JobService,
  private hiringProcessService: HiringProcessService,
  private cvAnalysisService: CvAnalysisService,
  private notificationService: NotificationService,
  public dialog: MatDialog
) {
  // Setup auto-search with debounce
  this.searchSubject.pipe(
    debounceTime(400),
    distinctUntilChanged()
  ).subscribe(searchTerm => {
    this.historyPage = 1;
    this.loadAnalysisHistory();
  });
}

  ngOnInit(): void {
    this.loadAnalysisHistory();
  }

  loadAnalysisHistory(): void {
    this.loadingHistory = true;
    this.cvAnalysisService.getHistory(this.historyPage, this.historyPageSize, this.searchQuery).subscribe({
      next: (response) => {
        this.analysisHistory = response.items;
        this.historyTotalCount = response.count;
        this.loadingHistory = false;
      },
      error: (error) => {
        this.loadingHistory = false;
        console.error('Error loading analysis history:', error);
      }
    });
  }

  onSearchChange(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  viewHistoricalAnalysis(historyItem: CvAnalysisHistoryItem): void {
    this.analyzing = true;
    this.cvAnalysisService.getAnalysisDetail(historyItem.analysisId).subscribe({
      next: (response) => {
        this.analyzing = false;
        this.matches = response.matches;
        this.totalPositions = response.totalPositionsAnalyzed;
        this.candidateInfo = response.candidateInfo;
        this.selectedFile = null;
      },
      error: (error) => {
        this.analyzing = false;
        this.notificationService.error(
          error.error?.message || 'Error loading analysis'
        );
      }
    });
  }

  dropped(files: NgxFileDropEntry[]): void {
    this.dragOver = false;
    for (const droppedFile of files) {
      if (droppedFile.fileEntry.isFile) {
        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {
          this.handleFile(file);
        });
      }
    }
  }

  fileOver(event: any): void {
    this.dragOver = true;
  }

  fileLeave(event: any): void {
    this.dragOver = false;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.handleFile(file);
    }
  }

  private handleFile(file: File): void {
    if (file.type !== 'application/pdf') {
      this.notificationService.warning('Please select a PDF file');
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.notificationService.warning('File size must be less than 10MB');
      return;
    }

    this.selectedFile = file;
  }

  analyzeCv(): void {
    if (!this.selectedFile) {
      return;
    }

    this.analyzing = true;
    this.matches = [];
    this.candidateInfo = null;

    this.jobService.analyzeCv(this.selectedFile).subscribe({
      next: (response) => {
        this.analyzing = false;
        this.matches = response.matches;
        this.totalPositions = response.totalPositionsAnalyzed;
        this.candidateInfo = response.candidateInfo || null;
        this.notificationService.success(
          `Found ${response.matchesFound} matches out of ${response.totalPositionsAnalyzed} positions`
        );
        // Reload history to show new analysis
        this.loadAnalysisHistory();
      },
      error: (error) => {
        this.analyzing = false;
        this.notificationService.error(
          error.error?.message || 'Error analyzing CV. Please try again.'
        );
      }
    });
  }

  reAnalyzeCv(): void {
    if (!this.candidateInfo?.candidateId) {
      return;
    }

    this.analyzing = true;
    this.matches = [];

    this.jobService.reAnalyzeCv(this.candidateInfo.candidateId).subscribe({
      next: (response) => {
        this.analyzing = false;
        this.matches = response.matches;
        this.totalPositions = response.totalPositionsAnalyzed;
        this.candidateInfo = response.candidateInfo || null;
        this.notificationService.success(
          `Found ${response.matchesFound} matches out of ${response.totalPositionsAnalyzed} positions`
        );
        // Reload history to show updated analysis
        this.loadAnalysisHistory();
      },
      error: (error) => {
        this.analyzing = false;
        this.notificationService.error(
          error.error?.message || 'Error re-analyzing CV. Please try again.'
        );
      }
    });
  }

  clearFile(): void {
    this.selectedFile = null;
    this.matches = [];
    this.candidateInfo = null;
  }

  viewResume(): void {
    if (!this.candidateInfo) {
      return;
    }

    import('../resume-viewer/resume-viewer-dialog.component').then(m => {
      this.dialog.open(m.ResumeViewerDialogComponent, {
        data: {
          candidateId: this.candidateInfo!.candidateId,
          candidateName: this.candidateInfo!.name || 'Candidate'
        },
        width: '90vw',
        height: '90vh',
        maxWidth: '1200px',
        panelClass: 'resume-viewer-dialog'
      });
    });
  }

  getMatchColor(percentage: number): string {
    if (percentage >= 80) return 'primary';
    if (percentage >= 70) return 'accent';
    if (percentage >= 40) return 'warn';
    return '';
  }

  startHiringProcess(match: JobMatch): void {
    console.log('Starting hiring process for match:', match);
    
    if (!this.candidateInfo) {
      this.notificationService.warning('Candidate information not available');
      return;
    }

    if (!match.jobPositionId) {
      console.error('Job position ID is missing from match:', match);
      this.notificationService.error('Job position information not available. Please analyze the CV again.');
      return;
    }

    console.log('Proceeding with referral - Candidate:', this.candidateInfo.candidateId, 'Job:', match.jobPositionId);

    // Show confirmation dialog
    import('./refer-confirmation-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.ReferConfirmationDialogComponent, {
        data: {
          candidateName: this.candidateInfo!.name || 'Unknown Candidate',
          jobTitle: match.jobTitle,
          matchPercentage: match.matchPercentage
        },
        width: '500px',
        maxWidth: '95vw',
        disableClose: false,
        autoFocus: true
      });

      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          console.log('User confirmed referral, calling API...');
          // User confirmed, proceed with referral
          this.hiringProcessService.startHiringProcess({
            candidateId: this.candidateInfo!.candidateId,
            jobPositionId: match.jobPositionId!,
            referralReason: match.referralReason
          }).subscribe({
            next: (response) => {
              console.log('Referral successful:', response);
              this.notificationService.success(
                `Candidate referral submitted successfully! Admin will review and decide whether to start the hiring process.`
              );
            },
            error: (error) => {
              console.error('Referral error:', error);
              const message = error.error?.message || 'Error submitting referral. It may already exist.';
              this.notificationService.error(message);
            }
          });
        } else {
          console.log('User cancelled referral');
        }
      });
    });
  }
}
