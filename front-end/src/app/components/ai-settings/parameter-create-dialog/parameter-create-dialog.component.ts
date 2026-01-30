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
      <mat-icon>add</mat-icon>
      Create New Parameter
    </h2>

    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Key</mat-label>
        <input matInput [(ngModel)]="key" placeholder="EXAMPLE_PARAMETER" (input)="onKeyInput()">
        <mat-hint>Use UPPERCASE with underscores (e.g., MY_PARAMETER)</mat-hint>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Value</mat-label>
        <input matInput [(ngModel)]="value" placeholder="Parameter value" maxlength="500">
        <mat-hint>Maximum 500 characters</mat-hint>
      </mat-form-field>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onCreate()" [disabled]="!isValid()">
        <mat-icon>add</mat-icon>
        Create
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 20px;
    }

    mat-dialog-content {
      min-width: 450px;
      padding: 24px 0;
    }

    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 24px 0;
      padding: 0 24px;
      font-size: 20px;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.87);
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
      border-top: 1px solid #e0e0e0;
      background-color: #fafafa;
    }

    button[mat-raised-button] {
      min-width: 100px;
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
