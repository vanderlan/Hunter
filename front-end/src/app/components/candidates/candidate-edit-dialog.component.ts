import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Candidate } from '../../models/candidate.model';

@Component({
  selector: 'app-candidate-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>edit</mat-icon>
      Edit Candidate
    </h2>

    <mat-dialog-content>
      <form [formGroup]="candidateForm" class="candidate-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter candidate name">
          <mat-icon matSuffix>person</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" placeholder="candidate@example.com">
          <mat-icon matSuffix>email</mat-icon>
          <mat-error *ngIf="candidateForm.get('email')?.hasError('email')">
            Please enter a valid email address
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Phone Number</mat-label>
          <input matInput formControlName="phoneNumber" placeholder="Enter phone number">
          <mat-icon matSuffix>phone</mat-icon>
        </mat-form-field>

        <div class="info-section">
          <p class="info-text">
            <mat-icon class="info-icon">info</mat-icon>
            <strong>Note:</strong> CV content cannot be edited. To update the CV, please re-upload and analyze.
          </p>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="saving">
        Cancel
      </button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onSave()" 
        [disabled]="!candidateForm.valid || saving">
        <mat-spinner *ngIf="saving" diameter="20" class="button-spinner"></mat-spinner>
        <span *ngIf="!saving">Save Changes</span>
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .candidate-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      min-width: 400px;
      padding: 20px 0;
    }

    .full-width {
      width: 100%;
    }

    .info-section {
      background-color: #e3f2fd;
      border-left: 4px solid #2196f3;
      padding: 12px;
      border-radius: 4px;
      margin-top: 8px;
    }

    .info-text {
      margin: 0;
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      color: #1976d2;
    }

    .info-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .button-spinner {
      display: inline-block;
      margin-right: 8px;
    }

    mat-dialog-content {
      overflow: visible;
    }

    mat-form-field {
      mat-icon {
        color: rgba(0, 0, 0, 0.54);
      }
    }
  `]
})
export class CandidateEditDialogComponent {
  candidateForm: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CandidateEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Candidate
  ) {
    this.candidateForm = this.fb.group({
      name: [data.name || '', [Validators.maxLength(200)]],
      email: [data.email || '', [Validators.email]],
      phoneNumber: [data.phoneNumber || '', [Validators.maxLength(50)]]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.candidateForm.valid) {
      this.saving = true;
      const updatedCandidate: Candidate = {
        ...this.data,
        name: this.candidateForm.value.name,
        email: this.candidateForm.value.email,
        phoneNumber: this.candidateForm.value.phoneNumber
      };
      this.dialogRef.close(updatedCandidate);
    }
  }
}
