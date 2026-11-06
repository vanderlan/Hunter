import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Candidate } from '../../models/candidate.model';

@Component({
  selector: 'app-candidate-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>person</mat-icon>
      Candidate Details
    </h2>
    <mat-dialog-content>
      <div class="detail-section">
        <div class="detail-row">
          <span class="detail-label">Name:</span>
          <span class="detail-value">{{ data.name || 'N/A' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Email:</span>
          <span class="detail-value">{{ data.email || 'N/A' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Phone Number:</span>
          <span class="detail-value">{{ data.phoneNumber || 'N/A' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Analyzed At:</span>
          <span class="detail-value">{{ data.analyzedAt | date: 'medium' }}</span>
        </div>
        <div class="detail-row">
          <span class="detail-label">Analyzed By:</span>
          <span class="detail-value">{{ data.analyzedByUserName }}</span>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      color: var(--hunter-brand);
      margin-bottom: 20px;

      mat-icon {
        color: var(--hunter-brand);
      }
    }

    mat-dialog-content {
      padding: 0 24px 24px;
      min-width: 400px;
    }

    .detail-section {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .detail-row {
      display: flex;
      gap: 12px;
      padding: 12px;
      background-color: var(--hunter-surface-variant);
      border-radius: 4px;
      border-left: 3px solid var(--hunter-brand);
    }

    .detail-label {
      font-weight: 600;
      color: var(--hunter-text-primary);
      min-width: 140px;
    }

    .detail-value {
      color: var(--hunter-text-secondary);
      flex: 1;
      word-break: break-word;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid var(--hunter-border);
    }

    @media (max-width: 600px) {
      mat-dialog-content {
        min-width: unset;
      }

      .detail-row {
        flex-direction: column;
        gap: 8px;
      }

      .detail-label {
        min-width: unset;
      }
    }
  `]
})
export class CandidateDetailDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<CandidateDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Candidate
  ) {}

  close(): void {
    this.dialogRef.close();
  }
}
