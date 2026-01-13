import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ReferConfirmationData {
  candidateName: string;
  jobTitle: string;
  matchPercentage: number;
}

@Component({
  selector: 'app-refer-confirmation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="refer-dialog">
      <div class="dialog-header">
        <mat-icon class="warning-icon">help_outline</mat-icon>
        <h2 mat-dialog-title>Confirm Candidate Referral</h2>
      </div>
      
      <mat-dialog-content>
        <div class="confirmation-message">
          <p class="main-message">
            Are you sure you want to refer <strong>{{ data.candidateName }}</strong> for the position:
          </p>
          <div class="job-details">
            <mat-icon>work</mat-icon>
            <div class="job-info">
              <strong>{{ data.jobTitle }}</strong>
              <span class="match-badge" [class.high-match]="data.matchPercentage >= 80">
                {{ data.matchPercentage }}% Match
              </span>
            </div>
          </div>
          <p class="info-message">
            <mat-icon class="info-icon">info</mat-icon>
            This will notify the admin team to review the candidate for the hiring process.
          </p>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">
          <mat-icon>close</mat-icon>
          Cancel
        </button>
        <button mat-raised-button color="primary" (click)="onConfirm()" cdkFocusInitial>
          <mat-icon>check_circle</mat-icon>
          Yes, Refer Candidate
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .refer-dialog {
      min-width: 400px;
      max-width: 500px;
      padding: 20px;
      @media (max-width: 600px) {
        min-width: 280px;
      }
    }

    .dialog-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 8px;

      .warning-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        color: #ff9800;
      }

      h2 {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: #333;
      }
    }

    mat-dialog-content {
      padding: 16px 0;
      margin: 0;
    }

    .confirmation-message {
      .main-message {
        font-size: 16px;
        line-height: 1.6;
        color: #333;
        margin: 0 0 16px;

        strong {
          color: #5f1f1f;
          font-weight: 600;
        }
      }

      .job-details {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 16px;
        background: #f5f5f5;
        border-radius: 8px;
        border-left: 4px solid #5f1f1f;
        margin-bottom: 16px;

        mat-icon {
          color: #5f1f1f;
          font-size: 28px;
          width: 28px;
          height: 28px;
        }

        .job-info {
          flex: 1;
          display: flex;
          flex-direction: column;
          gap: 6px;

          strong {
            font-size: 16px;
            color: #333;
          }

          .match-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            background: #e0e0e0;
            color: #666;
            font-size: 13px;
            font-weight: 600;
            width: fit-content;

            &.high-match {
              background: #e8f5e9;
              color: #2e7d32;
            }
          }
        }
      }

      .info-message {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        padding: 12px;
        background: #e3f2fd;
        border-radius: 6px;
        font-size: 14px;
        line-height: 1.5;
        color: #0d47a1;
        margin: 0;

        .info-icon {
          font-size: 20px;
          width: 20px;
          height: 20px;
          color: #1976d2;
          flex-shrink: 0;
          margin-top: 2px;
        }
      }
    }

    mat-dialog-actions {
      padding: 16px 0 0;
      margin: 0;
      gap: 12px;

      button {
        mat-icon {
          margin-right: 6px;
          font-size: 20px;
          width: 20px;
          height: 20px;
        }
      }
    }
  `]
})
export class ReferConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ReferConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReferConfirmationData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
