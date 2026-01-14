import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CvAnalysisDetail } from '../../services/cv-analysis.service';

@Component({
  selector: 'app-ai-analysis-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatChipsModule,
    MatExpansionModule,
    MatProgressBarModule
  ],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>
        <mat-icon>analytics</mat-icon>
        AI Analysis Results
      </h2>
      <button mat-icon-button mat-dialog-close>
        <mat-icon>close</mat-icon>
      </button>
    </div>

    <mat-dialog-content>
      <div class="analysis-header">
        <div class="header-info">
          <h3>{{ data.candidateInfo.name || 'Candidate' }}</h3>
          <p class="email">{{ data.candidateInfo.email }}</p>
          <p class="analysis-meta">
            <mat-icon>calendar_today</mat-icon>
            Analyzed on {{ data.analyzedAt | date:'medium' }} by {{ data.analyzedByUserName }}
          </p>
          <p class="stats">
            <mat-icon>work</mat-icon>
            {{ data.matchesFound }} matches found out of {{ data.totalPositionsAnalyzed }} positions analyzed
          </p>
        </div>
      </div>

      <div class="matches-section">
        <h3 class="section-title">Job Matches</h3>
        
        <div class="matches-grid">
          <mat-card *ngFor="let match of data.matches" class="match-card">
            <mat-card-header>
              <mat-card-title>{{ match.jobTitle }}</mat-card-title>
              <mat-card-subtitle>{{ match.companyCode }} - {{ match.jobCode }}</mat-card-subtitle>
            </mat-card-header>

            <mat-card-content>
              <div class="match-percentage">
                <h3>{{ match.matchPercentage }}% Match</h3>
                <mat-progress-bar 
                  mode="determinate" 
                  [value]="match.matchPercentage"
                  [color]="getMatchColor(match.matchPercentage)">
                </mat-progress-bar>
              </div>

              <p class="match-reason">{{ match.matchReason }}</p>

              <mat-expansion-panel>
                <mat-expansion-panel-header>
                  <mat-panel-title>Skills Analysis</mat-panel-title>
                </mat-expansion-panel-header>

                <div class="skills-section">
                  <h4>Matching Skills</h4>
                  <mat-chip-set>
                    <mat-chip *ngFor="let skill of match.matchingSkills" class="skill-match">
                      {{ skill }}
                    </mat-chip>
                  </mat-chip-set>
                </div>

                <div class="skills-section">
                  <h4>Skills to Develop</h4>
                  <mat-chip-set>
                    <mat-chip *ngFor="let skill of match.missingSkills" class="skill-missing">
                      {{ skill }}
                    </mat-chip>
                  </mat-chip-set>
                </div>
              </mat-expansion-panel>

              <mat-expansion-panel *ngIf="match.whatIsMissingForPerfectMatch">
                <mat-expansion-panel-header>
                  <mat-panel-title>What is missing for a perfect match?</mat-panel-title>
                </mat-expansion-panel-header>
                <p>{{ match.whatIsMissingForPerfectMatch }}</p>
              </mat-expansion-panel>

              <mat-expansion-panel *ngIf="match.referralReason">
                <mat-expansion-panel-header>
                  <mat-panel-title>Referral Reason</mat-panel-title>
                </mat-expansion-panel-header>
                <p>{{ match.referralReason }}</p>
              </mat-expansion-panel>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-raised-button color="primary" mat-dialog-close>
        <mat-icon>check</mat-icon>
        Close
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 20px 24px 0;

      h2 {
        display: flex;
        align-items: center;
        gap: 12px;
        color: #333;
        margin: 0;
        font-size: 24px;
        font-weight: 500;

        mat-icon {
          color: #5f1f1f;
          font-size: 28px;
          width: 28px;
          height: 28px;
        }
      }
    }

    mat-dialog-content {
      padding: 32px;
      max-height: 70vh;
      overflow-y: auto;
    }

    .analysis-header {
      background: linear-gradient(135deg, #f5f7fa 0%, #e8f0fe 100%);
      padding: 24px;
      border-radius: 8px;
      margin-bottom: 32px;
      border-left: 4px solid #5f1f1f;

      .header-info {
        h3 {
          margin: 0 0 8px 0;
          color: #5f1f1f;
          font-size: 24px;
          font-weight: 600;
        }

        .email {
          margin: 0 0 12px 0;
          color: #666;
          font-size: 16px;
        }

        .analysis-meta,
        .stats {
          display: flex;
          align-items: center;
          gap: 8px;
          margin: 8px 0;
          color: #555;
          font-size: 14px;

          mat-icon {
            font-size: 18px;
            width: 18px;
            height: 18px;
            color: #5f1f1f;
          }
        }
      }
    }

    .matches-section {
      .section-title {
        color: #333;
        font-size: 22px;
        font-weight: 600;
        margin: 0 0 24px 0;
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .matches-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
        gap: 24px;

        @media (max-width: 768px) {
          grid-template-columns: 1fr;
        }
      }

      .match-card {
        display: flex;
        flex-direction: column;
        height: 100%;

        mat-card-header {
          background: linear-gradient(135deg, #5f1f1f 0%, #81261d 100%);
          color: white;
          padding: 20px;
          border-radius: 5px;

          mat-card-title {
            color: white;
            font-size: 18px;
            font-weight: 500;
            margin: 0;
          }

          mat-card-subtitle {
            color: rgba(255, 255, 255, 0.9);
            font-size: 14px;
            margin: 8px 0 0 0;
          }
        }

        mat-card-content {
          padding: 24px 16px;
          flex: 1;
        }

        .match-percentage {
          margin-bottom: 20px;

          h3 {
            font-size: 24px;
            font-weight: 600;
            margin: 0 0 12px 0;
            color: #5f1f1f;
          }

          mat-progress-bar {
            height: 12px;
            border-radius: 6px;
          }
        }

        .match-reason {
          padding: 16px;
          background-color: #f5f5f5;
          border-radius: 8px;
          border-left: 4px solid #5f1f1f;
          margin-bottom: 20px;
          line-height: 1.6;
          color: #333;
          font-size: 14px;
        }

        mat-expansion-panel {
          margin-bottom: 8px;
          box-shadow: none !important;
          border: 1px solid #e0e0e0;

          &::before {
            display: none;
          }

          mat-expansion-panel-header {
            background-color: #fafafa;
            font-weight: 500;

            &:hover {
              background-color: #f5f5f5;
            }
          }

          mat-panel-title {
            font-size: 14px;
            color: #333;
          }

          .skills-section {
            margin-bottom: 24px;

            &:last-child {
              margin-bottom: 0;
            }

            h4 {
              font-size: 14px;
              font-weight: 600;
              margin: 0 0 12px 0;
              color: #666;
              text-transform: uppercase;
              letter-spacing: 0.5px;
            }

            mat_chip_set {
              display: flex;
              flex-wrap: wrap;
              gap: 8px;
            }

            .skill-match {
              background-color: #c8e6c9 !important;
              color: #2e7d32 !important;
            }

            .skill-missing {
              background-color: #ffcdd2 !important;
              color: #c62828 !important;
            }
          }

          p {
            padding: 16px 0;
            margin: 0;
            line-height: 1.6;
            color: #666;
          }
        }
      }
    }

    mat-dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
    }
  `]
})
export class AIAnalysisDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<AIAnalysisDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CvAnalysisDetail
  ) {}

  getMatchColor(percentage: number): string {
    if (percentage >= 80) return 'primary';
    if (percentage >= 70) return 'accent';
    if (percentage >= 40) return 'warn';
    return '';
  }
}
