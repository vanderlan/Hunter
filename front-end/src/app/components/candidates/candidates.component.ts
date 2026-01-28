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
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { CandidateService } from '../../services/candidate.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { Candidate } from '../../models/candidate.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
    selector: 'app-candidates',
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
    private authService: AuthService,
    private notificationService: NotificationService,
    public dialog: MatDialog
  ) {
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
          this.notificationService.error('Error loading candidates');
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
              this.notificationService.success('Candidate updated successfully');
              this.loadCandidates();
            },
            error: (error) => {
              this.loading = false;
              this.notificationService.error(error.error?.message || 'Error updating candidate');
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
        this.notificationService.warning('Only PDF files are allowed');
        input.value = '';
        return;
      }

      // Validate file size (10MB)
      if (file.size > 10 * 1024 * 1024) {
        this.notificationService.warning('File size must not exceed 10 MB');
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
        this.notificationService.success('Resume uploaded successfully');
        this.loadCandidates();
      },
      error: (error) => {
        this.uploadingResumeFor = null;
        this.notificationService.error(error.error?.message || 'Error uploading resume');
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
        this.notificationService.error('Error downloading resume');
      }
    });
  }

  canDeleteCandidate(candidate: Candidate): boolean {
    const isAdmin = this.authService.isAdmin();
    
    if (isAdmin) {
      return true;
    }
    
    return !candidate.hasHiringProcess;
  }

  deleteCandidate(candidate: Candidate): void {
    if (!this.canDeleteCandidate(candidate)) {
      this.notificationService.warning(
        'Only Admin users can delete candidates with existing hiring processes'
      );
      return;
    }

    import('./candidate-delete-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CandidateDeleteDialogComponent, {
        data: {
          candidate: candidate,
          hasHiringProcess: candidate.hasHiringProcess || false,
          isAdmin: this.authService.isAdmin()
        },
        width: '550px',
        maxWidth: '95vw',
        disableClose: true,
        autoFocus: true,
        panelClass: 'candidate-delete-dialog'
      });

      dialogRef.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.loading = true;
          this.candidateService.deleteCandidate(candidate.publicId).subscribe({
            next: () => {
              this.loading = false;
              this.notificationService.success('Candidate deleted successfully');
              this.loadCandidates();
            },
            error: (error) => {
              this.loading = false;
              const message = error.error?.message || error.message || 'Error deleting candidate';
              this.notificationService.error(message);
            }
          });
        }
      });
    });
  }
}
