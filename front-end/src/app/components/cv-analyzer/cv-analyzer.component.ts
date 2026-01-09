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
import { NgxFileDropModule, NgxFileDropEntry, FileSystemFileEntry } from 'ngx-file-drop';
import { JobService } from '../../services/job.service';
import { JobMatch, JobPosition } from '../../models/job.model';

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

  // Available positions section
  availablePositions: JobPosition[] = [];
  loadingPositions = false;

  constructor(
    private jobService: JobService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.fetchAvailablePositions();
  }

  fetchAvailablePositions(): void {
    this.loadingPositions = true;
    this.jobService.getActiveJobs().subscribe({
      next: (positions) => {
        this.availablePositions = positions;
        this.loadingPositions = false;
      },
      error: (error) => {
        this.loadingPositions = false;
        console.error('Error loading positions:', error);
      }
    });
  }

  openPositionDetails(position: JobPosition): void {
    import('./position-details-dialog.component').then(m => {
      this.dialog.open(m.PositionDetailsDialogComponent, {
        data: position,
        width: '800px',
        maxWidth: '95vw',
        panelClass: 'position-details-dialog-container'
      });
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

    this.jobService.analyzeCv(this.selectedFile).subscribe({
      next: (response) => {
        this.analyzing = false;
        this.matches = response.matches;
        this.totalPositions = response.totalPositionsAnalyzed;
        this.snackBar.open(
          `Found ${response.matchesFound} matches out of ${response.totalPositionsAnalyzed} positions`,
          'Close',
          { duration: 5000 }
        );
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
  }

  getMatchColor(percentage: number): string {
    if (percentage >= 80) return 'primary';
    if (percentage >= 60) return 'accent';
    if (percentage >= 40) return 'warn';
    return '';
  }
}
