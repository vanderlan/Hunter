import { Component, Inject } from '@angular/core';

import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CandidateService } from '../../services/candidate.service';

export interface ResumeViewerData {
  candidateId: string;
  candidateName: string;
}

@Component({
    selector: 'app-resume-viewer',
    imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
],
    templateUrl: './resume-viewer-dialog.component.html',
    styleUrl: './resume-viewer-dialog.component.scss'
})
export class ResumeViewerDialogComponent {
  resumeUrl: SafeResourceUrl | null = null;
  loading = true;
  error = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ResumeViewerData,
    private dialogRef: MatDialogRef<ResumeViewerDialogComponent>,
    private candidateService: CandidateService,
    private sanitizer: DomSanitizer
  ) {
    this.loadResume();
  }

  loadResume(): void {
    this.loading = true;
    this.error = false;

    this.candidateService.downloadResume(this.data.candidateId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        this.resumeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading resume:', error);
        this.error = true;
        this.loading = false;
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  download(): void {
    this.candidateService.downloadResume(this.data.candidateId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${this.data.candidateName}_Resume.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading resume:', error);
      }
    });
  }
}
