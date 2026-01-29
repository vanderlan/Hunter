import { Component, Inject } from '@angular/core';

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
    imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatIconModule
],
    template: `
    <h2 mat-dialog-title>Edit Candidate</h2>
    
    <mat-dialog-content>
      <form [formGroup]="candidateForm" class="candidate-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter candidate name">
        </mat-form-field>
    
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" placeholder="candidate@example.com">
          @if (candidateForm.get('email')?.hasError('email')) {
            <mat-error>
              Please enter a valid email address
            </mat-error>
          }
        </mat-form-field>
    
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Phone Number</mat-label>
          <input matInput formControlName="phoneNumber" placeholder="Enter phone number">
        </mat-form-field>
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
        @if (saving) {
          <mat-spinner diameter="20"></mat-spinner>
        }
        @if (!saving) {
          <span>Save</span>
        }
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

    mat-dialog-content {
      overflow: visible;
    }

    mat-dialog-actions {
      padding: 16px;
      border-top: 1px solid #e0e0e0;

      button {
        mat-spinner {
          display: inline-block;
          margin-right: 8px;
        }
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
