import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ConfirmEmailResponse } from '../../../models/public.model';
import { PublicService } from '../../../services/public.service';
import { extractApiError } from '../../../utils/api-error';

@Component({
  selector: 'app-public-confirm-email',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="public-page confirm-page">
      <div class="confirm-card">
        @if (loading) {
          <mat-spinner diameter="48"></mat-spinner>
          <h1>Confirming your email...</h1>
        }

        @if (!loading && result) {
          <mat-icon class="confirm-icon success">verified</mat-icon>
          <h1>{{ result.isAlreadyConfirmed ? 'Already confirmed' : 'Email confirmed' }}</h1>
          <p>
            Thanks{{ result.name ? ', ' + result.name : '' }}. Your profile is now visible to the people inside Hunter.
            @if (result.pendingReferralRequests > 0) {
              Your {{ result.pendingReferralRequests }} referral request{{ result.pendingReferralRequests !== 1 ? 's were' : ' was' }} delivered.
            }
          </p>
          <div class="confirm-actions">
            <a mat-flat-button color="primary" routerLink="/match">Back to my matches</a>
            <a mat-stroked-button routerLink="/">Open positions</a>
          </div>
        }

        @if (!loading && errorMessage) {
          <mat-icon class="confirm-icon error">link_off</mat-icon>
          <h1>We could not confirm this link</h1>
          <p>{{ errorMessage }}</p>
          <div class="confirm-actions">
            <a mat-flat-button color="primary" routerLink="/match">Go to CV match</a>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    @use '../public-shared' as *;

    .confirm-page {
      padding-top: var(--hunter-space-16);
      padding-bottom: var(--hunter-space-16);
      display: flex;
      justify-content: center;
    }

    .confirm-card {
      width: min(560px, 100%);
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--hunter-space-4);
      text-align: center;
      padding: var(--hunter-space-10) var(--hunter-space-8);
      background: var(--hunter-surface);
      border: 1px solid var(--hunter-border);
      border-radius: var(--hunter-radius-xl);

      h1 { margin: 0; font-size: 1.5rem; }
      p { margin: 0; color: var(--hunter-text-secondary); }
    }

    .confirm-icon {
      font-size: 56px;
      width: 56px;
      height: 56px;

      &.success { color: var(--hunter-success-fg); }
      &.error { color: var(--hunter-error-fg); }
    }

    .confirm-actions {
      display: flex;
      gap: var(--hunter-space-3);
      flex-wrap: wrap;
      justify-content: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicConfirmEmailComponent implements OnInit {
  loading = true;
  result: ConfirmEmailResponse | null = null;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private publicService: PublicService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.loading = false;
      this.errorMessage = 'This link is missing its confirmation code.';
      return;
    }

    this.publicService.confirmEmail(token).subscribe({
      next: (result) => {
        this.result = result;
        this.loading = false;

        const session = this.publicService.loadSession();
        if (session) {
          session.isEmailConfirmed = true;
          this.publicService.saveSession(session);
        }
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = extractApiError(error, 'The link is invalid or has expired.');
      }
    });
  }
}
