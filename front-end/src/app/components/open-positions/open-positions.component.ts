import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { JobService } from '../../services/job.service';
import { NotificationService } from '../../services/notification.service';
import { JobPosition } from '../../models/job.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-open-positions',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule
  ],
  templateUrl: './open-positions.component.html',
  styleUrl: './open-positions.component.scss'
})
export class OpenPositionsComponent implements OnInit {
  positions: JobPosition[] = [];
  filteredPositions: JobPosition[] = [];
  loading = false;
  searchQuery = '';
  private searchSubject = new Subject<string>();

  constructor(
    private jobService: JobService,
    private notificationService: NotificationService,
    private route: ActivatedRoute,
    public dialog: MatDialog
  ) {
    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.filterPositions();
    });
  }

  ngOnInit(): void {
    this.loadPositions();

    // Check for jobId query parameter
    this.route.queryParams.subscribe(params => {
      const jobId = params['jobId'];
      if (jobId && this.positions.length > 0) {
        this.openPositionByPublicId(jobId);
      }
    });
  }

  loadPositions(): void {
    this.loading = true;
    this.jobService.getActiveJobs().subscribe({
      next: (positions) => {
        this.positions = positions;
        this.filteredPositions = positions;
        this.loading = false;

        // Check again for jobId after positions are loaded
        const jobId = this.route.snapshot.queryParams['jobId'];
        if (jobId) {
          this.openPositionByPublicId(jobId);
        }
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading positions');
        console.error('Error loading positions:', error);
      }
    });
  }

  onSearchChange(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  filterPositions(): void {
    if (!this.searchQuery.trim()) {
      this.filteredPositions = this.positions;
      return;
    }

    const searchLower = this.searchQuery.toLowerCase();
    this.filteredPositions = this.positions.filter(position => 
      position.title.toLowerCase().includes(searchLower) ||
      position.companyCode.toLowerCase().includes(searchLower) ||
      position.jobCode.toLowerCase().includes(searchLower) ||
      (position.summary && position.summary.toLowerCase().includes(searchLower))
    );
  }

  openPositionDetails(position: JobPosition): void {
    import('../cv-analyzer/position-details-dialog.component').then(m => {
      this.dialog.open(m.PositionDetailsDialogComponent, {
        data: position,
        width: '800px',
        maxWidth: '95vw',
        panelClass: 'position-details-dialog-container'
      });
    });
  }

  openPositionByPublicId(publicId: string): void {
    const position = this.positions.find(p => p.publicId === publicId);
    if (position) {
      this.openPositionDetails(position);
    }
  }
}
