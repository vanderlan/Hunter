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
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JobPositionService } from '../../services/job-position.service';
import { JobPosition } from '../../models/job.model';
import { NotificationService } from '../../services/notification.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

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
  private searchSubject = new Subject<string>();

  constructor(
    private jobPositionService: JobPositionService,
    private notificationService: NotificationService,
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

    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.pageIndex = 0;
      this.loadJobPositions();
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
          this.notificationService.error('Error loading job positions');
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadJobPositions();
  }

  onSearchChange(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  showCreateForm(): void {
    this.isEditing = true;
    this.currentJobPositionId = null;
    this.jobPositionForm.reset({ isActive: true });
  }

  editJobPosition(jobPosition: JobPosition): void {
    this.isEditing = true;
    this.currentJobPositionId = jobPosition.publicId!;
    this.loading = true;
    
    // Fetch the latest data from API using public ID
    this.jobPositionService.getJobPositionById(jobPosition.publicId!)
      .subscribe({
        next: (data) => {
          this.jobPositionForm.patchValue(data);
          this.loading = false;
        },
        error: (error) => {
          this.loading = false;
          this.notificationService.error('Error loading job position details');
          this.cancelEdit();
        }
      });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.currentJobPositionId = null;
    this.jobPositionForm.reset({ isActive: true });
  }

  saveJobPosition(): void {
    if (this.jobPositionForm.invalid) {
      this.notificationService.warning('Please fill in all required fields');
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
            this.notificationService.success('Job position updated successfully');
            this.cancelEdit();
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.notificationService.error(error.error?.message || 'Error updating job position');
          }
        });
    } else {
      // Create
      this.jobPositionService.createJobPosition(jobPosition)
        .subscribe({
          next: () => {
            this.loading = false;
            this.notificationService.success('Job position created successfully');
            this.cancelEdit();
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.notificationService.error(error.error?.message || 'Error creating job position');
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
            this.notificationService.success('Job position deleted successfully');
            this.loadJobPositions();
          },
          error: (error) => {
            this.loading = false;
            this.notificationService.error('Error deleting job position');
          }
        });
    }
  }
}
