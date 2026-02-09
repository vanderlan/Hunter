import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HiringProcessService } from '../../services/hiring-process.service';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { HiringProcessListItem, DashboardStats, HiringProcessStatusHelper } from '../../models/hiring-process.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
    selector: 'app-hiring-process',
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        MatPaginatorModule,
        MatChipsModule,
        MatProgressSpinnerModule,
        MatFormFieldModule,
        MatInputModule,
        MatSnackBarModule,
        FormsModule
    ],
    templateUrl: './hiring-process.component.html',
    styleUrl: './hiring-process.component.scss'
})
export class HiringProcessComponent implements OnInit {
  processes: HiringProcessListItem[] = [];
  loading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  dashboard: DashboardStats | null = null;
  private searchSubject = new Subject<string>();
  
  displayedColumns: string[] = [
    'candidateName',
    'jobTitle',
    'status',
    'createdByUserName',
    'createdAt',
    'actions'
  ];

  constructor(
    private hiringProcessService: HiringProcessService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.pageIndex = 0;
      this.loadProcesses();
    });
  }

  ngOnInit(): void {
    this.loadDashboard();
    this.loadProcesses();
  }

  loadDashboard(): void {
    this.hiringProcessService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
      }
    });
  }

  loadProcesses(): void {
    this.loading = true;
    this.hiringProcessService.getHiringProcesses(
      this.pageIndex + 1,
      this.pageSize,
      undefined,
      undefined,
      undefined,
      this.searchQuery || undefined
    ).subscribe({
      next: (data) => {
        this.processes = data.items;
        this.totalCount = data.count;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading hiring processes');
        console.error('Error loading processes:', error);
      }
    });
  }

  getStatusPriority(status: string): number {
    return HiringProcessStatusHelper.getPriority(status);
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadProcesses();
  }

  onSearchChange(searchTerm: string): void {
    this.searchQuery = searchTerm;
    this.searchSubject.next(searchTerm);
  }

  viewProcess(process: HiringProcessListItem): void {
    this.router.navigate(['/hiring-process', process.publicId]);
  }

  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  getReferralsLabel(): string {
    return this.isAdmin() ? 'Referrals' : 'My Referrals';
  }

  getStatusColor(status: string): string {
    return HiringProcessStatusHelper.getCssClass(status);
  }

  getStatusLabel(status: string): string {
    return HiringProcessStatusHelper.getLabel(status);
  }
}
