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
import { CandidateService } from '../../services/candidate.service';
import { Candidate } from '../../models/candidate.model';

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
    MatTooltipModule
  ],
  templateUrl: './candidates.component.html',
  styleUrl: './candidates.component.scss'
})
export class CandidatesComponent implements OnInit {
  displayedColumns: string[] = ['name', 'email', 'analyzedAt', 'analyzedByUserName', 'actions'];
  dataSource: Candidate[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  loading = false;

  constructor(
    private candidateService: CandidateService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {}

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

  onSearch(): void {
    this.pageIndex = 0;
    this.loadCandidates();
  }

  viewCandidate(candidate: Candidate): void {
    this.loading = true;
    this.candidateService.getCandidateById(candidate.publicId).subscribe({
      next: (data) => {
        this.loading = false;
        this.openCandidateDialog(data);
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open('Error loading candidate details', 'Close', { duration: 3000 });
      }
    });
  }

  openCandidateDialog(candidate: Candidate): void {
    import('./candidate-detail-dialog.component').then(m => {
      this.dialog.open(m.CandidateDetailDialogComponent, {
        data: candidate,
        width: '500px'
      });
    });
  }
}
