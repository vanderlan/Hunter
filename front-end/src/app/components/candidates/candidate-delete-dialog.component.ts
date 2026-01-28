import { Component, Inject } from '@angular/core';

import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Candidate } from '../../models/candidate.model';

export interface CandidateDeleteDialogData {
  candidate: Candidate;
  hasHiringProcess: boolean;
  isAdmin: boolean;
}

@Component({
    selector: 'app-candidate-delete-dialog',
    imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule
],
    template: `
    <div class="delete-dialog">
      <h2 mat-dialog-title>
        <mat-icon class="warning-icon">warning</mat-icon>
        Confirm Deletion
      </h2>
    
      <mat-dialog-content>
        <p class="confirmation-message">
          Are you sure you want to delete candidate
          <strong>{{ data.candidate.name || data.candidate.email || 'Unknown' }}</strong>?
        </p>
    
        @if (data.hasHiringProcess) {
          <div class="warning-box">
            <mat-icon>info</mat-icon>
            <div>
              <strong>Warning:</strong> This candidate has existing hiring processes.
              All related data will be permanently deleted including:
              <ul>
                <li>CV Analysis records</li>
                <li>Job matches</li>
                <li>Hiring process history</li>
                <li>Status changes and notes</li>
                <li>Uploaded files</li>
              </ul>
            </div>
          </div>
        }
    
        @if (!data.hasHiringProcess) {
          <div class="info-box">
            <mat-icon>info</mat-icon>
            <div>
              The following data will be permanently deleted:
              <ul>
                <li>Candidate information</li>
                <li>CV Analysis records</li>
                <li>Job matches</li>
                <li>Uploaded files</li>
              </ul>
            </div>
          </div>
        }
    
        <p class="final-warning">
          <strong>This action cannot be undone.</strong>
        </p>
      </mat-dialog-content>
    
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-raised-button color="warn" (click)="onConfirm()">
          <mat-icon>delete</mat-icon>
          Delete Candidate
        </button>
      </mat-dialog-actions>
    </div>
    `,
    styles: [`
    .delete-dialog {
      font-family: Roboto, sans-serif;
      min-width: 450px;
      max-width: 600px;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #f44336;
      margin: 0 0 20px 0;
      padding: 0;
      font-size: 20px;
      font-weight: 500;
    }

    .warning-icon {
      color: #f44336;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    mat-dialog-content {
      padding: 0 24px 20px 24px;
      margin: 0;
      max-height: 70vh;
      overflow-y: auto;
    }

    .confirmation-message {
      font-size: 16px;
      margin: 0 0 20px 0;
      line-height: 1.5;
      color: rgba(0, 0, 0, 0.87);
    }

    .warning-box,
    .info-box {
      display: flex;
      gap: 12px;
      padding: 16px;
      border-radius: 8px;
      margin: 0 0 20px 0;
      align-items: flex-start;
    }

    .warning-box {
      background-color: #fff3e0;
      border-left: 4px solid #ff9800;
    }

    .warning-box mat-icon {
      color: #ff9800;
    }

    .info-box {
      background-color: #e3f2fd;
      border-left: 4px solid #2196f3;
    }

    .info-box mat-icon {
      color: #2196f3;
    }

    .warning-box mat-icon,
    .info-box mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
      flex-shrink: 0;
      margin-top: 2px;
    }

    .warning-box > div,
    .info-box > div {
      flex: 1;
      font-size: 14px;
      line-height: 1.6;
    }

    ul {
      margin: 12px 0 0 0;
      padding-left: 20px;
    }

    li {
      margin: 6px 0;
      line-height: 1.4;
    }

    .final-warning {
      color: #d32f2f;
      font-size: 14px;
      margin: 20px 0 0 0;
      text-align: center;
      padding: 12px;
      background-color: #ffebee;
      border-radius: 4px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
      border-top: 1px solid rgba(0, 0, 0, 0.12);
      gap: 8px;
    }

    button {
      text-transform: uppercase;
      font-weight: 500;
    }

    button mat-icon {
      margin-right: 4px;
    }

    @media (max-width: 600px) {
      .delete-dialog {
        min-width: unset;
        max-width: 100%;
      }

      mat-dialog-content {
        padding: 0 16px 16px 16px;
      }

      mat-dialog-actions {
        padding: 12px 16px;
      }
    }
  `]
})
export class CandidateDeleteDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<CandidateDeleteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CandidateDeleteDialogData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
