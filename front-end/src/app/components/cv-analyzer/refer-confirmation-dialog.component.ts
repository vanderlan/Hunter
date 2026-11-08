import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';

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
    imports: [
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
    changeDetection: ChangeDetectionStrategy.Eager,
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
        color: var(--hunter-warn-fg);
      }

      h2 {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: var(--hunter-text-primary);
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
        color: var(--hunter-text-primary);
        margin: 0 0 16px;

        strong {
          color: var(--hunter-brand);
          font-weight: 600;
        }
      }

      .job-details {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 16px;
        background: var(--hunter-surface-variant);
        border-radius: 8px;
        border-left: 4px solid var(--hunter-brand);
        margin-bottom: 16px;

        mat-icon {
          color: var(--hunter-brand);
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
            color: var(--hunter-text-primary);
          }

          .match-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            background: var(--hunter-surface-hover);
            color: var(--hunter-text-secondary);
            font-size: 13px;
            font-weight: 600;
            width: fit-content;

            &.high-match {
              background: var(--hunter-success-bg);
              color: var(--hunter-success-fg);
            }
          }
        }
      }

      .info-message {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        padding: 12px;
        background: var(--hunter-info-bg);
        border-radius: 6px;
        font-size: 14px;
        line-height: 1.5;
        color: var(--hunter-info-fg);
        margin: 0;

        .info-icon {
          font-size: 20px;
          width: 20px;
          height: 20px;
          color: var(--hunter-info-fg);
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
