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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HiringProcessService } from '../../services/hiring-process.service';
import { HiringProcessListItem, DashboardStats } from '../../models/hiring-process.model';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-hiring-process',
  standalone: true,
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
  currentPage = 1;
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
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    // Setup auto-search with debounce
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.currentPage = 1;
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
      this.currentPage,
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
        this.snackBar.open('Error loading hiring processes', 'Close', { duration: 3000 });
        console.error('Error loading processes:', error);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
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

  getStatusColor(status: string): string {
    switch (status) {
      case 'Hired':
        return 'primary';
      case 'Reproved':
        return 'warn';
      case 'Screening':
      case 'ConsultantTechnicalInterview':
      case 'ClientInterview':
        return 'accent';
      default:
        return '';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'ConsultantTechnicalInterview':
        return 'Consultant Technical Interview';
      case 'ClientInterview':
        return 'Client Interview';
      default:
        return status;
    }
  }
}
