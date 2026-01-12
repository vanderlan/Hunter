import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { HiringProcessService } from '../../services/hiring-process.service';
import { AuthService } from '../../services/auth.service';
import { HiringProcess, HiringProcessStatus } from '../../models/hiring-process.model';

@Component({
  selector: 'app-hiring-process-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    FormsModule
  ],
  templateUrl: './hiring-process-detail.component.html',
  styleUrl: './hiring-process-detail.component.scss'
})
export class HiringProcessDetailComponent implements OnInit {
  process: HiringProcess | null = null;
  loading = true;
  newNote = '';
  selectedStatus: HiringProcessStatus | null = null;
  statusNotes = '';
  isAdmin = false;

  allStatusOptions = [
    { value: HiringProcessStatus.Referred, label: 'Referred' },
    { value: HiringProcessStatus.Screening, label: 'Screening' },
    { value: HiringProcessStatus.ConsultantTechnicalInterview, label: 'Consultant Technical Interview' },
    { value: HiringProcessStatus.ClientInterview, label: 'Client Interview' },
    { value: HiringProcessStatus.Hired, label: 'Hired' },
    { value: HiringProcessStatus.Reproved, label: 'Reproved' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiringProcessService: HiringProcessService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProcess(id);
    }
  }

  loadProcess(id: string): void {
    this.loading = true;
    this.hiringProcessService.getHiringProcessById(id).subscribe({
      next: (data) => {
        this.process = data;
        // Map string status from API to numeric enum for the select dropdown
        this.selectedStatus = this.mapStringToEnum(data.status);
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open('Error loading hiring process', 'Close', { duration: 3000 });
        console.error('Error loading process:', error);
        this.router.navigate(['/hiring-process']);
      }
    });
  }

  mapStringToEnum(status: string): HiringProcessStatus {
    switch (status) {
      case 'Referred':
        return HiringProcessStatus.Referred;
      case 'Screening':
        return HiringProcessStatus.Screening;
      case 'ConsultantTechnicalInterview':
        return HiringProcessStatus.ConsultantTechnicalInterview;
      case 'ClientInterview':
        return HiringProcessStatus.ClientInterview;
      case 'Hired':
        return HiringProcessStatus.Hired;
      case 'Reproved':
        return HiringProcessStatus.Reproved;
      default:
        return HiringProcessStatus.Referred;
    }
  }

  updateStatus(): void {
    if (!this.process || !this.selectedStatus || !this.isAdmin) {
      return;
    }

    const currentStatusEnum = this.mapStringToEnum(this.process.status);
    if (this.selectedStatus === currentStatusEnum) {
      this.snackBar.open('Please select a different status', 'Close', { duration: 3000 });
      return;
    }

    this.hiringProcessService.updateStatus(this.process.publicId, {
      newStatus: this.selectedStatus,
      notes: this.statusNotes || undefined
    }).subscribe({
      next: () => {
        this.snackBar.open('Status updated successfully', 'Close', { duration: 3000 });
        this.statusNotes = '';
        this.loadProcess(this.process!.publicId);
      },
      error: (error) => {
        this.snackBar.open('Error updating status', 'Close', { duration: 3000 });
        console.error('Error updating status:', error);
      }
    });
  }

  addNote(): void {
    if (!this.process || !this.newNote.trim()) {
      return;
    }

    this.hiringProcessService.addNote(this.process.publicId, {
      note: this.newNote.trim()
    }).subscribe({
      next: () => {
        this.snackBar.open('Note added successfully', 'Close', { duration: 3000 });
        this.newNote = '';
        this.loadProcess(this.process!.publicId);
      },
      error: (error) => {
        this.snackBar.open('Error adding note', 'Close', { duration: 3000 });
        console.error('Error adding note:', error);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/hiring-process']);
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

  get availableStatusOptions() {
    if (!this.process) {
      return this.allStatusOptions;
    }
    
    const currentStatusEnum = this.mapStringToEnum(this.process.status);
    return this.allStatusOptions.filter(option => option.value !== currentStatusEnum);
  }
}
