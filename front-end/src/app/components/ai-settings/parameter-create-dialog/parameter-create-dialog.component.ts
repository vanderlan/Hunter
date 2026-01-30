import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ParameterCreateRequest } from '../../../services/parameter.service';

@Component({
  selector: 'app-parameter-create-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>tune</mat-icon>
      Create New Parameter
    </h2>

    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Parameter Key</mat-label>
        <input matInput [(ngModel)]="key" placeholder="EXAMPLE_PARAMETER" (input)="onKeyInput()" autocomplete="off">
        <mat-hint>Use UPPERCASE with underscores (e.g., MY_PARAMETER)</mat-hint>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Parameter Value</mat-label>
        <input matInput [(ngModel)]="value" placeholder="Parameter value" maxlength="500" autocomplete="off">
        <mat-hint>Maximum 500 characters</mat-hint>
      </mat-form-field>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">
        <mat-icon>close</mat-icon>
        Cancel
      </button>
      <button mat-raised-button color="primary" (click)="onCreate()" [disabled]="!isValid()">
        <mat-icon>add_circle</mat-icon>
        Create
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 24px 0;
      font-size: 24px;
      font-weight: 500;

      mat-icon {
        font-size: 28px;
        width: 28px;
        height: 28px;
      }
    }

    .full-width {
      width: 100%;
      margin-bottom: 20px;
    }

    mat-dialog-content {
      min-width: 500px;
      padding: 20px 0;
      overflow: visible;
    }

    mat-dialog-actions {
      padding: 16px;
      margin: 0;
      border-top: 1px solid #e0e0e0;
      gap: 8px;

      button {
        min-width: 100px;

        mat-icon {
          margin-right: 4px;
          font-size: 20px;
          width: 20px;
          height: 20px;
        }
      }
    }
  `]
})
export class ParameterCreateDialogComponent {
  key = '';
  value = '';

  constructor(
    private dialogRef: MatDialogRef<ParameterCreateDialogComponent>
  ) {}

  onKeyInput(): void {
    // Auto-convert to uppercase and remove invalid characters
    this.key = this.key.toUpperCase().replace(/[^A-Z_]/g, '');
  }

  isValid(): boolean {
    return this.key.trim().length > 0 && 
           this.value.trim().length > 0 &&
           /^[A-Z_]+$/.test(this.key);
  }

  onCreate(): void {
    if (this.isValid()) {
      const request: ParameterCreateRequest = {
        key: this.key.trim(),
        value: this.value.trim()
      };
      this.dialogRef.close(request);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
