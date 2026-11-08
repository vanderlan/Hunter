import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface CancelConfirmationData {
  candidateName: string;
  jobTitle: string;
}

export interface CancelConfirmationResult {
  confirmed: boolean;
  reason?: string;
}

@Component({
    selector: 'app-cancel-confirmation-dialog',
    imports: [
        CommonModule,
        FormsModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatFormFieldModule,
        MatInputModule
    ],
    template: `
    <div class="cancel-dialog">
      <div class="dialog-header">
        <mat-icon class="warning-icon">warning</mat-icon>
        <h2 mat-dialog-title>Cancel Referral</h2>
      </div>
      
      <mat-dialog-content>
        <div class="confirmation-message">
          <p class="main-message">
            Are you sure you want to cancel the referral for <strong>{{ data.candidateName }}</strong>?
          </p>
          <div class="job-details">
            <mat-icon>work</mat-icon>
            <div class="job-info">
              <strong>{{ data.jobTitle }}</strong>
            </div>
          </div>
          <p class="warning-message">
            <mat-icon class="warning-icon-small">error_outline</mat-icon>
            This action cannot be undone. All related data including status history and notes will be permanently deleted.
          </p>
          
          <mat-form-field appearance="outline" class="full-width reason-field">
            <mat-label>Cancellation Reason (Required)</mat-label>
            <textarea 
              matInput 
              [(ngModel)]="cancelReason" 
              rows="4" 
              maxlength="1000"
              placeholder="Please explain why you are cancelling this referral..."
              required></textarea>
            <mat-hint>This reason will be sent to all admin users</mat-hint>
          </mat-form-field>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">
          <mat-icon>close</mat-icon>
          Keep Referral
        </button>
        <button 
          mat-raised-button 
          color="warn" 
          (click)="onConfirm()" 
          [disabled]="!cancelReason.trim()"
          cdkFocusInitial>
          <mat-icon>delete_forever</mat-icon>
          Yes, Cancel Referral
        </button>
      </mat-dialog-actions>
    </div>
  `,
    changeDetection: ChangeDetectionStrategy.Eager,
    styles: [`
    .cancel-dialog {
      min-width: 450px;
      max-width: 550px;
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
        color: #f44336;
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
        }
      }

      .warning-message {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        padding: 12px;
        background: var(--hunter-warn-bg);
        border-radius: 6px;
        font-size: 14px;
        line-height: 1.5;
        color: var(--hunter-warn-fg);
        margin: 0 0 20px;
        border-left: 3px solid #ff9800;

        .warning-icon-small {
          font-size: 20px;
          width: 20px;
          height: 20px;
          color: #ff9800;
          flex-shrink: 0;
          margin-top: 2px;
        }
      }

      .reason-field {
        margin-top: 16px;

        textarea {
          font-family: inherit;
        }
      }
    }

    .full-width {
      width: 100%;
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
export class CancelConfirmationDialogComponent {
  cancelReason = '';

  constructor(
    public dialogRef: MatDialogRef<CancelConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CancelConfirmationData
  ) {}

  onCancel(): void {
    this.dialogRef.close({ confirmed: false });
  }

  onConfirm(): void {
    if (this.cancelReason.trim()) {
      this.dialogRef.close({ 
        confirmed: true, 
        reason: this.cancelReason.trim() 
      });
    }
  }
}
