import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { JobService } from '../../services/job.service';
import { JobPosition } from '../../models/job.model';

@Component({
  selector: 'app-open-positions',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './open-positions.component.html',
  styleUrl: './open-positions.component.scss'
})
export class OpenPositionsComponent implements OnInit {
  positions: JobPosition[] = [];
  loading = false;

  constructor(
    private jobService: JobService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadPositions();
  }

  loadPositions(): void {
    this.loading = true;
    this.jobService.getActiveJobs().subscribe({
      next: (positions) => {
        this.positions = positions;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.snackBar.open('Error loading positions', 'Close', { duration: 3000 });
        console.error('Error loading positions:', error);
      }
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
}
