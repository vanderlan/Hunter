import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

export type ToastTone = 'success' | 'error' | 'warning' | 'info';

export interface ToastData {
  message: string;
  tone: ToastTone;
  /** Optional bold line rendered above the message. */
  title?: string;
}

const TONE_ICON: Record<ToastTone, string> = {
  success: 'check_circle',
  error: 'error',
  warning: 'warning',
  info: 'info',
};

/**
 * Toast body for NotificationService.
 *
 * Replaces the old approach of faking icons with Unicode glyphs in a
 * `::before` pseudo-element on a saturated colour slab. This renders real
 * Material icons on a soft tinted surface built from the shared status
 * tokens, so a "success" toast matches a "success" chip elsewhere.
 */
@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [MatIconModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast" [class]="'toast--' + data.tone" role="alert" aria-live="polite">
      <mat-icon class="toast__icon" aria-hidden="true">{{ icon }}</mat-icon>

      <div class="toast__body">
        @if (data.title) {
          <p class="toast__title">{{ data.title }}</p>
        }
        <p class="toast__message">{{ data.message }}</p>
      </div>

      <button
        type="button"
        class="toast__close"
        aria-label="Dismiss notification"
        (click)="dismiss()">
        <mat-icon aria-hidden="true">close</mat-icon>
      </button>
    </div>
  `,
  styleUrl: './notification-toast.component.scss',
})
export class NotificationToastComponent {
  readonly data = inject<ToastData>(MAT_SNACK_BAR_DATA);
  private readonly ref = inject<MatSnackBarRef<NotificationToastComponent>>(MatSnackBarRef);

  get icon(): string {
    return TONE_ICON[this.data.tone];
  }

  dismiss(): void {
    this.ref.dismiss();
  }
}
