import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JobPositionService } from '../../services/job-position.service';
import { JobPosition } from '../../models/job.model';

@Component({
  selector: 'app-job-positions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './job-positions.component.html',
  styleUrl: './job-positions.component.scss'
})
export class JobPositionsComponent implements OnInit {
  displayedColumns: string[] = ['title', 'companyCode', 'jobCode', 'isActive', 'actions'];
  dataSource: JobPosition[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  loading = false;
  isEditing = false;
  jobPositionForm: FormGroup;
  currentJobPositionId: string | null = null;

  constructor(
    private jobPositionService: JobPositionService,
    private snackBar: MatSnackBar,
    private fb: FormBuilder,
    public dialog: MatDialog
  ) {
    this.jobPositionForm = this.fb.group({
      title: ['', Validators.required],
      externalUrl: ['', Validators.required],
      companyCode: ['', Validators.required],
      jobCode: ['', Validators.required],
      summary: [''],
      responsibilities: [''],
      requirements: [''],
      mustHaveSkills: [''],
      niceToHaveSkills: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadJobPositions();
  }

  loadJobPositions(): void {
    this.loading = true;
    this.jobPositionService.getJobPositions(this.pageIndex + 1, this.pageSize, this.searchQuery)
      .subscribe({
        next: (response) => {
          this.dataSource = response.items;
          this.totalCount = response.count;
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Error loading job positions', 'Close', { duration: 3000 });
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadJobPositions();
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadJobPositions();
  }

  showCreateForm(): void {
    this.isEditing = true;
    this.currentJobPositionId = null;
    this.jobPositionForm.reset({ isActive: true });
  }

  editJobPosition(jobPosition: JobPosition): void {
    this.isEditing = true;
    this.currentJobPositionId = jobPosition.publicId!;
    this.jobPositionForm.patchValue(jobPosition);
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.currentJobPositionId = null;
    this.jobPositionForm.reset({ isActive: true });
  }

  saveJobPosition(): void {
    if (this.jobPositionForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    const jobPosition: JobPosition = this.jobPositionForm.value;
    this.loading = true;

    if (this.currentJobPositionId) {
      // Update
      this.jobPositionService.updateJobPosition(this.currentJobPositionId, jobPosition)
        .subscribe({
          next: () => {
            this.loading = false;
            this.snackBar.open('Job position updated successfully', 'Close', { duration: 3000 });
            this.cancelEdit();
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open(error.error?.message || 'Error updating job position', 'Close', { duration: 3000 });
          }
        });
    } else {
      // Create
      this.jobPositionService.createJobPosition(jobPosition)
        .subscribe({
          next: () => {
            this.loading = false;
            this.snackBar.open('Job position created successfully', 'Close', { duration: 3000 });
            this.cancelEdit();
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open(error.error?.message || 'Error creating job position', 'Close', { duration: 3000 });
          }
        });
    }
  }

  deleteJobPosition(id: string): void {
    if (confirm('Are you sure you want to delete this job position?')) {
      this.loading = true;
      this.jobPositionService.deleteJobPosition(id)
        .subscribe({
          next: () => {
            this.loading = false;
            this.snackBar.open('Job position deleted successfully', 'Close', { duration: 3000 });
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.snackBar.open('Error deleting job position', 'Close', { duration: 3000 });
          }
        });
    }
  }
}
