import { Component, OnInit, OnDestroy } from '@angular/core';

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
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { PriorityBadgeComponent } from '../shared/priority-badge.component';

@Component({
    selector: 'app-open-positions',
    imports: [
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    PriorityBadgeComponent
],
    templateUrl: './open-positions.component.html',
    styleUrl: './open-positions.component.scss'
})
export class OpenPositionsComponent implements OnInit, OnDestroy {
  positions: JobPosition[] = [];
  filteredPositions: JobPosition[] = [];
  loading = false;
  searchQuery = '';
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private jobService: JobService,
    private notificationService: NotificationService,
    private route: ActivatedRoute,
    public dialog: MatDialog
  ) {
    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.filterPositions();
    });
  }

  ngOnInit(): void {
    this.loadPositions();

    // Listen for jobId query parameter changes
    this.route.queryParams.pipe(
      takeUntil(this.destroy$)
    ).subscribe(params => {
      const jobId = params['jobId'];
      if (jobId && this.positions.length > 0) {
        this.openPositionByPublicId(jobId);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPositions(): void {
    this.loading = true;
    this.jobService.getActiveJobs().subscribe({
      next: (positions) => {
        this.positions = this.sortByPriority(positions);
        this.filteredPositions = this.positions;
        this.loading = false;

        // Check for jobId after positions are loaded
        this.checkAndOpenJobFromUrl();
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading positions');
        console.error('Error loading positions:', error);
      }
    });
  }

  private checkAndOpenJobFromUrl(): void {
    const jobId = this.route.snapshot.queryParams['jobId'];
    if (jobId) {
      this.openPositionByPublicId(jobId);
    }
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
    const filtered = this.positions.filter(position => 
      position.title.toLowerCase().includes(searchLower) ||
      position.companyCode.toLowerCase().includes(searchLower) ||
      position.jobCode.toLowerCase().includes(searchLower) ||
      (position.summary && position.summary.toLowerCase().includes(searchLower))
    );
    this.filteredPositions = this.sortByPriority(filtered);
  }

  private sortByPriority(positions: JobPosition[]): JobPosition[] {
    const priorityOrder: { [key: string]: number } = {
      'High': 1,
      'Medium': 2,
      'Low': 3
    };

    return positions.sort((a, b) => {
      const priorityA = priorityOrder[a.priority || 'Low'] || 999;
      const priorityB = priorityOrder[b.priority || 'Low'] || 999;
      return priorityA - priorityB;
    });
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
