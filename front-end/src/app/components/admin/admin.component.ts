import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { JobService } from '../../services/job.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent {
  isSyncing = false;
  lastSyncResult: any = null;

  constructor(
    private jobService: JobService,
    private notificationService: NotificationService
  ) {}

  syncJobs(): void {
    this.isSyncing = true;
    this.lastSyncResult = null;

    this.jobService.syncJobs().subscribe({
      next: (result) => {
        this.isSyncing = false;
        this.lastSyncResult = result;
        
        const message = `Sync completed: ${result.newPositions} new, ${result.updatedPositions} updated, ${result.closedPositions} closed`;
        this.notificationService.success(message);
      },
      error: (error) => {
        this.isSyncing = false;
        const errorMessage = error.error?.message || 'Error synchronizing jobs. Please try again.';
        this.notificationService.error(errorMessage);
      }
    });
  }
}
