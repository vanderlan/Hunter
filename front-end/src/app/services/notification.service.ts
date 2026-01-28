import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export enum NotificationType {
  SUCCESS = 'success',
  ERROR = 'error',
  WARNING = 'warning',
  INFO = 'info'
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  constructor(private snackBar: MatSnackBar) {}

  /**
   * Shows a success notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4000)
   */
  success(message: string, duration: number = 4000): void {
    this.show(message, NotificationType.SUCCESS, duration);
  }

  /**
   * Shows an error notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 5000)
   */
  error(message: string, duration: number = 5000): void {
    this.show(message, NotificationType.ERROR, duration);
  }

  /**
   * Shows a warning notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4500)
   */
  warning(message: string, duration: number = 4500): void {
    this.show(message, NotificationType.WARNING, duration);
  }

  /**
   * Shows an info notification
   * @param message The message to display
   * @param duration Duration in milliseconds (default: 4000)
   */
  info(message: string, duration: number = 4000): void {
    this.show(message, NotificationType.INFO, duration);
  }

  /**
   * Shows a notification with the specified type
   * @param message The message to display
   * @param type The notification type
   * @param duration Duration in milliseconds
   */
  private show(message: string, type: NotificationType, duration: number): void {
    const config: MatSnackBarConfig = {
      duration: duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: [`${type}-notification`, 'custom-notification']
    };

    this.snackBar.open(message, '', config);
  }
}
