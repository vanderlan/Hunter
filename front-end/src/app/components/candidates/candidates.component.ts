import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { CandidateService } from '../../services/candidate.service';
import { Candidate } from '../../models/candidate.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-candidates',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule
  ],
  templateUrl: './candidates.component.html',
  styleUrl: './candidates.component.scss'
})
export class CandidatesComponent implements OnInit {
  displayedColumns: string[] = ['name', 'email', 'phoneNumber', 'analyzedAt', 'analyzedByUserName', 'actions'];
  dataSource: Candidate[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  loading = false;
  uploadingResumeFor: string | null = null;
  private searchSubject = new Subject<string>();

  constructor(
    private candidateService: CandidateService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {
    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.pageIndex = 0;
      this.loadCandidates();
    });
  }

  ngOnInit(): void {
    this.loadCandidates();
  }

  loadCandidates(): void {
    this.loading = true;
    this.candidateService.getCandidates(this.pageIndex + 1, this.pageSize, this.searchQuery)
      .subscribe({
        next: (response) => {
          this.dataSource = response.items;
          this.totalCount = response.count;
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Error loading candidates', 'Close', { duration: 3000 });
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadCandidates();
  }

  onSearchChange(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  editCandidate(candidate: Candidate): void {
    import('./candidate-edit-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CandidateEditDialogComponent, {
        data: candidate,
        width: '500px',
        disableClose: false
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.loading = true;
          this.candidateService.updateCandidate(result.publicId, result).subscribe({
            next: () => {
              this.loading = false;
              this.snackBar.open('Candidate updated successfully', 'Close', { duration: 3000 });
              this.loadCandidates();
            },
            error: (error) => {
              this.loading = false;
              this.snackBar.open(error.error?.message || 'Error updating candidate', 'Close', { duration: 3000 });
            }
          });
        }
      });
    });
  }

  viewResume(candidate: Candidate): void {
    import('../resume-viewer/resume-viewer-dialog.component').then(m => {
      this.dialog.open(m.ResumeViewerDialogComponent, {
        data: {
          candidateId: candidate.publicId,
          candidateName: candidate.name || 'Unknown'
        },
        width: '90vw',
        height: '90vh',
        maxWidth: '1200px',
        panelClass: 'resume-viewer-dialog'
      });
    });
  }

  onResumeFileSelected(event: Event, candidate: Candidate): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (file.type !== 'application/pdf') {
        this.snackBar.open('Only PDF files are allowed', 'Close', { duration: 3000 });
        input.value = '';
        return;
      }

      // Validate file size (10MB)
      if (file.size > 10 * 1024 * 1024) {
        this.snackBar.open('File size must not exceed 10 MB', 'Close', { duration: 3000 });
        input.value = '';
        return;
      }

      this.uploadResume(candidate, file);
      input.value = '';
    }
  }

  uploadResume(candidate: Candidate, file: File): void {
    this.uploadingResumeFor = candidate.publicId;
    
    this.candidateService.uploadResume(candidate.publicId, file).subscribe({
      next: (response) => {
        this.uploadingResumeFor = null;
        this.snackBar.open('Resume uploaded successfully', 'Close', { duration: 3000 });
        this.loadCandidates();
      },
      error: (error) => {
        this.uploadingResumeFor = null;
        this.snackBar.open(error.error?.message || 'Error uploading resume', 'Close', { duration: 3000 });
      }
    });
  }

  triggerFileUpload(candidate: Candidate): void {
    const fileInput = document.getElementById(`resume-upload-${candidate.publicId}`) as HTMLInputElement;
    if (fileInput) {
      fileInput.click();
    }
  }

  downloadResume(candidate: Candidate): void {
    this.candidateService.downloadResume(candidate.publicId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${candidate.name || 'Candidate'}_Resume.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.snackBar.open('Error downloading resume', 'Close', { duration: 3000 });
      }
    });
  }
}
