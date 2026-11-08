import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import {
  NotificationToastComponent,
  ToastData,
  ToastTone,
} from '../components/shared/notification-toast/notification-toast.component';

export enum NotificationType {
  SUCCESS = 'success',
  ERROR = 'error',
  WARNING = 'warning',
  INFO = 'info'
}

/** Optional extras; omitting them preserves the previous behaviour exactly. */
export interface NotificationOptions {
  /** Bold line rendered above the message. */
  title?: string;
  /** Override the default duration for this tone. */
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  /**
   * Shows a success notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4000)
   */
  success(message: string, duration: number = 4000, options?: NotificationOptions): void {
    this.show(message, NotificationType.SUCCESS, duration, options);
  }

  /**
   * Shows an error notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 5000)
   */
  error(message: string, duration: number = 5000, options?: NotificationOptions): void {
    this.show(message, NotificationType.ERROR, duration, options);
  }

  /**
   * Shows a warning notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4500)
   */
  warning(message: string, duration: number = 4500, options?: NotificationOptions): void {
    this.show(message, NotificationType.WARNING, duration, options);
  }

  /**
   * Shows an info notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4000)
   */
  info(message: string, duration: number = 4000, options?: NotificationOptions): void {
    this.show(message, NotificationType.INFO, duration, options);
  }

  /**
   * Shows a notification with the specified type
   * @param message The message to display
   * @param type The notification type
   * @param duration Duration in milliseconds
   */
  private show(
    message: string,
    type: NotificationType,
    duration: number,
    options?: NotificationOptions,
  ): void {
    const config: MatSnackBarConfig<ToastData> = {
      duration: options?.duration ?? duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      // `hunter-toast-panel` strips the default snackbar chrome so the
      // component below owns its own surface, shape and elevation.
      panelClass: ['hunter-toast-panel', `${type}-notification`],
      data: { message, tone: type as ToastTone, title: options?.title },
    };

    this.snackBar.openFromComponent(NotificationToastComponent, config);
  }
}
