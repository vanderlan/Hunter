import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { NgxFileDropModule, NgxFileDropEntry, FileSystemFileEntry } from 'ngx-file-drop';
import { JobService } from '../../services/job.service';
import { HiringProcessService } from '../../services/hiring-process.service';
import { CvAnalysisService, CvAnalysisHistoryItem } from '../../services/cv-analysis.service';
import { JobMatch, CandidateInfo } from '../../models/job.model';

@Component({
  selector: 'app-cv-analyzer',
  standalone: true,
  imports: [
    CommonModule,
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
  historyPageSize = 10;
  historyTotalCount = 0;

  constructor(
    private jobService: JobService,
    private hiringProcessService: HiringProcessService,
    private cvAnalysisService: CvAnalysisService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadAnalysisHistory();
  }

  loadAnalysisHistory(): void {
    this.loadingHistory = true;
    this.cvAnalysisService.getHistory(this.historyPage, this.historyPageSize).subscribe({
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

  viewHistoricalAnalysis(historyItem: CvAnalysisHistoryItem): void {
    this.analyzing = true;
    this.cvAnalysisService.getAnalysisDetail(historyItem.analysisId).subscribe({
      next: (response) => {
        this.analyzing = false;
        this.matches = response.matches;
        this.totalPositions = response.totalPositionsAnalyzed;
        this.candidateInfo = response.candidateInfo;
        this.selectedFile = null;
        this.snackBar.open(
          `Loaded analysis from ${new Date(response.analyzedAt).toLocaleDateString()}`,
          'Close',
          { duration: 3000 }
        );
      },
      error: (error) => {
        this.analyzing = false;
        this.snackBar.open(
          error.error?.message || 'Error loading analysis',
          'Close',
          { duration: 5000 }
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
      this.snackBar.open('Please select a PDF file', 'Close', { duration: 3000 });
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.snackBar.open('File size must be less than 10MB', 'Close', { duration: 3000 });
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
        this.snackBar.open(
          `Found ${response.matchesFound} matches out of ${response.totalPositionsAnalyzed} positions`,
          'Close',
          { duration: 5000 }
        );
        // Reload history to show new analysis
        this.loadAnalysisHistory();
      },
      error: (error) => {
        this.analyzing = false;
        this.snackBar.open(
          error.error?.message || 'Error analyzing CV. Please try again.',
          'Close',
          { duration: 5000 }
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
      this.snackBar.open('Candidate information not available', 'Close', { duration: 3000 });
      return;
    }

    if (!match.jobPositionId) {
      console.error('Job position ID is missing from match:', match);
      this.snackBar.open('Job position information not available. Please analyze the CV again.', 'Close', { duration: 5000 });
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
              this.snackBar.open(
                `Candidate referral submitted successfully! Admin will review and decide whether to start the hiring process.`,
                'Close',
                { duration: 6000 }
              );
            },
            error: (error) => {
              console.error('Referral error:', error);
              const message = error.error?.message || 'Error submitting referral. It may already exist.';
              this.snackBar.open(message, 'Close', { duration: 5000 });
            }
          });
        } else {
          console.log('User cancelled referral');
        }
      });
    });
  }
}
