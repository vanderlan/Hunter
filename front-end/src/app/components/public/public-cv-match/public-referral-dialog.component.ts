import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CreateReferralResponse, PublicJobMatch } from '../../../models/public.model';
import { PublicService } from '../../../services/public.service';
import { extractApiError } from '../../../utils/api-error';

export interface PublicReferralDialogData {
  candidateToken: string;
  match: PublicJobMatch;
  isEmailConfirmed: boolean;
}

@Component({
  selector: 'app-public-referral-dialog',
  imports: [DecimalPipe, FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatProgressSpinnerModule],
  template: `
    <div class="referral-dialog">
      <h2 mat-dialog-title>
        <mat-icon>waving_hand</mat-icon>
        Ask for a referral
      </h2>

      <mat-dialog-content>
        <div class="referral-job">
          <mat-icon>work</mat-icon>
          <div>
            <strong>{{ data.match.jobTitle }}</strong>
            <span class="referral-match">{{ data.match.matchPercentage | number:'1.0-0' }}% match</span>
          </div>
        </div>

        <p class="referral-intro">
          People inside Hunter will see your CV, your match and the message below. If one of them
          knows the team, they can refer you directly.
        </p>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Message (optional)</mat-label>
          <textarea matInput rows="4" maxlength="1000" [(ngModel)]="message"
            placeholder="Why you are a good fit, your availability, anything that helps."></textarea>
          <mat-hint align="end">{{ message.length }}/1000</mat-hint>
        </mat-form-field>

        @if (!data.isEmailConfirmed) {
          <div class="referral-note">
            <mat-icon>mark_email_unread</mat-icon>
            Your request is delivered once you confirm your email. Check your inbox after sending.
          </div>
        }

        @if (errorMessage) {
          <div class="referral-error">
            <mat-icon>error_outline</mat-icon>
            {{ errorMessage }}
          </div>
        }
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="dialogRef.close()" [disabled]="sending">Cancel</button>
        <button mat-flat-button color="primary" (click)="send()" [disabled]="sending" cdkFocusInitial>
          @if (sending) {
            <mat-spinner diameter="18"></mat-spinner>
          } @else {
            <span>Send request</span>
          }
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .referral-dialog {
      min-width: min(520px, 92vw);
      padding: var(--hunter-space-2);
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: var(--hunter-space-2);
      mat-icon { color: var(--hunter-brand); }
    }

    .referral-job {
      display: flex;
      align-items: center;
      gap: var(--hunter-space-3);
      padding: var(--hunter-space-3) var(--hunter-space-4);
      background: var(--hunter-surface-variant);
      border-left: 4px solid var(--hunter-brand);
      border-radius: var(--hunter-radius-sm);
      margin-bottom: var(--hunter-space-4);

      mat-icon { color: var(--hunter-brand); }

      div {
        display: flex;
        flex-direction: column;
        gap: 2px;
      }
    }

    .referral-match {
      font-size: 0.8125rem;
      color: var(--hunter-success-fg);
      font-weight: 600;
    }

    .referral-intro {
      color: var(--hunter-text-secondary);
      margin: 0 0 var(--hunter-space-4);
    }

    .full-width { width: 100%; }

    .referral-note,
    .referral-error {
      display: flex;
      align-items: flex-start;
      gap: var(--hunter-space-2);
      padding: var(--hunter-space-2) var(--hunter-space-3);
      border-radius: var(--hunter-radius-sm);
      font-size: 0.8125rem;
      margin-top: var(--hunter-space-2);

      mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }
    }

    .referral-note {
      background: var(--hunter-info-bg);
      color: var(--hunter-info-fg);
    }

    .referral-error {
      background: var(--hunter-error-bg);
      color: var(--hunter-error-fg);
    }
  `],
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicReferralDialogComponent {
  message = '';
  sending = false;
  errorMessage = '';

  constructor(
    public dialogRef: MatDialogRef<PublicReferralDialogComponent, CreateReferralResponse>,
    @Inject(MAT_DIALOG_DATA) public data: PublicReferralDialogData,
    private publicService: PublicService
  ) {}

  send(): void {
    if (this.sending) {
      return;
    }
    this.sending = true;
    this.errorMessage = '';

    this.publicService.requestReferral(this.data.candidateToken, {
      jobCode: this.data.match.jobCode,
      message: this.message.trim() || undefined
    }).subscribe({
      next: (response) => {
        this.sending = false;
        this.dialogRef.close(response);
      },
      error: (error) => {
        this.sending = false;
        this.errorMessage = extractApiError(error, 'We could not send your request. Please try again.');
      }
    });
  }
}
