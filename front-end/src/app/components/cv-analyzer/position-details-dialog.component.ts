import { Component, Inject } from '@angular/core';

import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { JobPosition } from '../../models/job.model';

@Component({
    selector: 'app-position-details-dialog',
    imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule
],
    template: `
    <div class="position-details-wrapper">
      <h2 mat-dialog-title class="position-details-header">
        <mat-icon class="position-details-icon">work_outline</mat-icon>
        <span class="position-details-header-text">{{ data.title }}</span>
      </h2>
    
      <mat-dialog-content class="position-details-body">
        <div class="position-meta-info">
          @if (data.externalUrl) {
            <div class="position-meta-item">
              <a [href]="data.externalUrl" target="_blank" rel="noopener noreferrer" class="external-link">
                <mat-icon class="position-meta-icon">open_in_new</mat-icon>
                <span class="position-meta-text">View External Link</span>
              </a>
            </div>
          }
        </div>

        <mat-divider class="position-section-divider"></mat-divider>
    
        @if (data.summary) {
          <div class="position-info-section">
            <h3 class="position-section-heading">
              <mat-icon class="position-section-icon">description</mat-icon>
              <span>Summary</span>
            </h3>
            <p class="position-section-text">{{ data.summary }}</p>
          </div>
        }
    
        @if (data.summary) {
          <mat-divider class="position-section-divider"></mat-divider>
        }
    
        @if (data.responsibilities) {
          <div class="position-info-section">
            <h3 class="position-section-heading">
              <mat-icon class="position-section-icon">assignment</mat-icon>
              <span>Responsibilities</span>
            </h3>
            <p class="position-section-text">{{ data.responsibilities }}</p>
          </div>
        }
    
        @if (data.responsibilities) {
          <mat-divider class="position-section-divider"></mat-divider>
        }
    
        @if (data.requirements) {
          <div class="position-info-section">
            <h3 class="position-section-heading">
              <mat-icon class="position-section-icon">checklist</mat-icon>
              <span>Requirements</span>
            </h3>
            <p class="position-section-text">{{ data.requirements }}</p>
          </div>
        }
    
        @if (data.requirements) {
          <mat-divider class="position-section-divider"></mat-divider>
        }
    
        @if (data.mustHaveSkills) {
          <div class="position-info-section">
            <h3 class="position-section-heading">
              <mat-icon class="position-section-icon">star</mat-icon>
              <span>Must-Have Skills</span>
            </h3>
            <p class="position-section-text">{{ data.mustHaveSkills }}</p>
          </div>
        }
    
        @if (data.mustHaveSkills) {
          <mat-divider class="position-section-divider"></mat-divider>
        }
    
        @if (data.niceToHaveSkills) {
          <div class="position-info-section">
            <h3 class="position-section-heading">
              <mat-icon class="position-section-icon">star_half</mat-icon>
              <span>Nice-to-Have Skills</span>
            </h3>
            <p class="position-section-text">{{ data.niceToHaveSkills }}</p>
          </div>
        }
      </mat-dialog-content>
    
      <mat-dialog-actions class="position-details-actions" align="end">
        <button mat-raised-button color="primary" (click)="closeDialog()" class="position-close-btn">
          Close
        </button>
      </mat-dialog-actions>
    </div>
    `,
    styles: [`
    .position-details-wrapper {
      .position-details-header {
        display: flex;
        align-items: center;
        gap: 12px;
        color: #5f1f1f;
        margin: 0;
        padding: 24px 24px 16px;
        border-bottom: 2px solid #e0e0e0;

        .position-details-icon {
          font-size: 28px;
          width: 28px;
          height: 28px;
          color: #5f1f1f;
        }

        .position-details-header-text {
          font-size: 22px;
          font-weight: 500;
          line-height: 1.3;
        }
      }

      .position-details-body {
        padding: 0;
        min-height: 300px;
        max-height: 70vh;
        overflow-y: auto;
      }

      .position-meta-info {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
        padding: 20px 24px;
        background-color: #f8f8f8;

        .position-meta-item {
          display: flex;
          align-items: center;
          gap: 8px;

          .position-meta-icon {
            font-size: 20px;
            width: 20px;
            height: 20px;
            color: #999;
          }

          .position-meta-text {
            font-size: 14px;
            color: #666;
            font-weight: 500;
          }
        }

        .external-link {
          display: flex;
          align-items: center;
          gap: 6px;
          text-decoration: none;
          color: #5f1f1f;
          transition: all 0.2s ease;

          .position-meta-icon {
            font-size: 18px;
            width: 18px;
            height: 18px;
            color: #5f1f1f;
          }

          .position-meta-text {
            font-size: 14px;
            line-height: 1.2;
          }

          &:hover {
            color: #8b2f2f;

            .position-meta-text {
              text-decoration: underline;
            }
          }
        }
      }

      .position-section-divider {
        margin: 0;
      }

      .position-info-section {
        padding: 24px;

        .position-section-heading {
          display: flex;
          align-items: center;
          gap: 10px;
          font-size: 16px;
          font-weight: 600;
          color: #333;
          margin: 0 0 16px;

          .position-section-icon {
            font-size: 22px;
            width: 22px;
            height: 22px;
            color: #5f1f1f;
          }
        }

        .position-section-text {
          margin: 0;
          color: #666;
          line-height: 1.8;
          white-space: pre-wrap;
          font-size: 14px;
        }
      }

      .position-details-actions {
        padding: 16px 24px;
        border-top: 1px solid #e0e0e0;
        margin: 0;

        .position-close-btn {
          min-width: 100px;
          font-weight: 500;
        }
      }
    }

    @media (max-width: 600px) {
      .position-details-wrapper {
        .position-details-header {
          padding: 20px 16px 12px;

          .position-details-header-text {
            font-size: 18px;
          }
        }

        .position-meta-info {
          padding: 16px;
          flex-direction: column;
          gap: 12px;
        }

        .position-info-section {
          padding: 20px 16px;
        }

        .position-details-actions {
          padding: 12px 16px;
        }
      }
    }
  `]
})
export class PositionDetailsDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<PositionDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: JobPosition
  ) {}

  closeDialog(): void {
    this.dialogRef.close();
  }
}
