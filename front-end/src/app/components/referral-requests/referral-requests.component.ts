import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ReferralRequest } from '../../models/referral-request.model';
import { NotificationService } from '../../services/notification.service';
import { ReferralRequestService } from '../../services/referral-request.service';
import { extractApiError } from '../../utils/api-error';
import { ReferralRequestResolveDialogComponent, ReferralRequestResolveDialogResult } from './referral-request-resolve-dialog.component';

/** Numeric values of ReferralRequestStatus on the API. */
const STATUS_FILTER: Record<string, number | undefined> = {
  all: undefined,
  pending: 1,
  accepted: 2,
  declined: 3
};

@Component({
  selector: 'app-referral-requests',
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatTooltipModule
  ],
  templateUrl: './referral-requests.component.html',
  styleUrl: './referral-requests.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class ReferralRequestsComponent implements OnInit {
  displayedColumns = ['candidate', 'job', 'match', 'createdAt', 'status', 'actions'];
  dataSource: ReferralRequest[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  statusFilter: keyof typeof STATUS_FILTER = 'pending';
  searchQuery = '';
  loading = false;
  resolvingId: string | null = null;

  private searchSubject = new Subject<string>();

  constructor(
    private referralRequestService: ReferralRequestService,
    private notificationService: NotificationService,
    private router: Router,
    private dialog: MatDialog
  ) {
    this.searchSubject.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.referralRequestService
      .getReferralRequests(this.pageIndex + 1, this.pageSize, STATUS_FILTER[this.statusFilter], this.searchQuery || undefined)
      .subscribe({
        next: (response) => {
          this.dataSource = response.items;
          this.totalCount = response.count;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.notificationService.error('Error loading referral requests');
        }
      });
  }

  onStatusFilterChange(value: string): void {
    this.statusFilter = value as keyof typeof STATUS_FILTER;
    this.pageIndex = 0;
    this.load();
  }

  onSearchChange(term: string): void {
    this.searchQuery = term;
    this.searchSubject.next(term);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  statusClass(status: string): string {
    return `status-badge status-${status.toLowerCase()}`;
  }

  viewResume(request: ReferralRequest): void {
    import('../resume-viewer/resume-viewer-dialog.component').then(m => {
      this.dialog.open(m.ResumeViewerDialogComponent, {
        data: {
          candidateId: request.candidateId,
          candidateName: request.candidateName
        },
        width: '90vw',
        maxWidth: '1200px',
        height: '90vh'
      });
    });
  }

  resolve(request: ReferralRequest, action: 'accept' | 'decline'): void {
    const dialogRef = this.dialog.open(ReferralRequestResolveDialogComponent, {
      data: { request, action },
      width: '560px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe((result?: ReferralRequestResolveDialogResult) => {
      if (!result) {
        return;
      }

      this.resolvingId = request.publicId;

      if (action === 'accept') {
        this.referralRequestService.accept(request.publicId, result.note).subscribe({
          next: (response) => {
            this.resolvingId = null;
            this.notificationService.success(`${request.candidateName || 'Candidate'} referred. Hiring process started.`);
            this.router.navigate(['/hiring-process', response.hiringProcessId]);
          },
          error: (error) => {
            this.resolvingId = null;
            this.notificationService.error(extractApiError(error, 'Could not accept the request'));
          }
        });
      } else {
        this.referralRequestService.decline(request.publicId, result.note).subscribe({
          next: () => {
            this.resolvingId = null;
            this.notificationService.success('Request declined');
            this.load();
          },
          error: (error) => {
            this.resolvingId = null;
            this.notificationService.error(extractApiError(error, 'Could not decline the request'));
          }
        });
      }
    });
  }
}
