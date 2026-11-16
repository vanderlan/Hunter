import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ReferralRequest } from '../../models/referral-request.model';

export interface ReferralRequestResolveDialogData {
  request: ReferralRequest;
  action: 'accept' | 'decline';
}

export interface ReferralRequestResolveDialogResult {
  note?: string;
}

@Component({
  selector: 'app-referral-request-resolve-dialog',
  imports: [DecimalPipe, FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  template: `
    <div class="resolve-dialog">
      <h2 mat-dialog-title>
        <mat-icon [class.accept]="data.action === 'accept'" [class.decline]="data.action === 'decline'">
          {{ data.action === 'accept' ? 'how_to_reg' : 'person_off' }}
        </mat-icon>
        {{ data.action === 'accept' ? 'Refer this candidate' : 'Decline request' }}
      </h2>

      <mat-dialog-content>
        <div class="resolve-summary">
          <div class="resolve-row">
            <span class="resolve-label">Candidate</span>
            <span class="resolve-value">{{ data.request.candidateName || 'Unknown' }} · {{ data.request.candidateEmail }}</span>
          </div>
          <div class="resolve-row">
            <span class="resolve-label">Position</span>
            <span class="resolve-value">{{ data.request.jobTitle }}</span>
          </div>
          @if (data.request.matchPercentage != null) {
            <div class="resolve-row">
              <span class="resolve-label">AI match</span>
              <span class="resolve-value">{{ data.request.matchPercentage | number:'1.0-0' }}%</span>
            </div>
          }
        </div>

        @if (data.action === 'accept') {
          <p class="resolve-info">
            <mat-icon>info</mat-icon>
            This starts a hiring process in your name, exactly like referring a candidate from the CV analyzer. Admins are notified.
          </p>
        } @else {
          <p class="resolve-info">
            <mat-icon>info</mat-icon>
            The candidate is not notified. The request stays in the list as declined.
          </p>
        }

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ data.action === 'accept' ? 'Note for the admin team (optional)' : 'Reason (optional)' }}</mat-label>
          <textarea matInput rows="3" maxlength="1000" [(ngModel)]="note"></textarea>
        </mat-form-field>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="dialogRef.close()">Cancel</button>
        <button mat-flat-button [color]="data.action === 'accept' ? 'primary' : 'warn'" (click)="confirm()" cdkFocusInitial>
          {{ data.action === 'accept' ? 'Refer candidate' : 'Decline' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .resolve-dialog { min-width: min(520px, 92vw); padding: var(--hunter-space-2); }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: var(--hunter-space-2);

      mat-icon.accept { color: var(--hunter-success-fg); }
      mat-icon.decline { color: var(--hunter-error-fg); }
    }

    .resolve-summary {
      display: flex;
      flex-direction: column;
      gap: var(--hunter-space-2);
      padding: var(--hunter-space-3) var(--hunter-space-4);
      background: var(--hunter-surface-variant);
      border-radius: var(--hunter-radius-sm);
      margin-bottom: var(--hunter-space-4);
    }

    .resolve-row { display: flex; gap: var(--hunter-space-3); font-size: 0.875rem; }
    .resolve-label { width: 90px; flex-shrink: 0; color: var(--hunter-text-hint); }
    .resolve-value { color: var(--hunter-text-primary); }

    .resolve-info {
      display: flex;
      align-items: flex-start;
      gap: var(--hunter-space-2);
      padding: var(--hunter-space-2) var(--hunter-space-3);
      background: var(--hunter-info-bg);
      color: var(--hunter-info-fg);
      border-radius: var(--hunter-radius-sm);
      font-size: 0.8125rem;
      margin: 0 0 var(--hunter-space-4);

      mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }
    }

    .full-width { width: 100%; }
  `],
  changeDetection: ChangeDetectionStrategy.Eager
})
export class ReferralRequestResolveDialogComponent {
  note = '';

  constructor(
    public dialogRef: MatDialogRef<ReferralRequestResolveDialogComponent, ReferralRequestResolveDialogResult>,
    @Inject(MAT_DIALOG_DATA) public data: ReferralRequestResolveDialogData
  ) {}

  confirm(): void {
    this.dialogRef.close({ note: this.note.trim() || undefined });
  }
}
