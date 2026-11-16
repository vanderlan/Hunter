import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RegisterPublicCandidateRequest, RegisterPublicCandidateResponse } from '../../../models/public.model';
import { PublicService } from '../../../services/public.service';
import { extractApiError } from '../../../utils/api-error';

export interface PublicRegisterDialogData {
  candidateToken: string;
  prefill?: Partial<RegisterPublicCandidateRequest>;
  /** Why we are asking: shown at the top of the dialog. */
  reason: 'save' | 'referral';
}

export interface PublicRegisterDialogResult {
  profile: RegisterPublicCandidateRequest;
  response: RegisterPublicCandidateResponse;
}

@Component({
  selector: 'app-public-register-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="register-dialog">
      <h2 mat-dialog-title>
        <mat-icon>person_add</mat-icon>
        {{ data.reason === 'referral' ? 'Who should we refer?' : 'Save my profile' }}
      </h2>

      <mat-dialog-content>
        <p class="register-intro">
          @if (data.reason === 'referral') {
            To ask for a referral we need a way for the person inside to reach you. Your CV stays private until then.
          } @else {
            Save your contact details so we can tell you when a new position fits your CV.
          }
        </p>

        <form [formGroup]="form" id="public-register-form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Full name</mat-label>
            <input matInput formControlName="name" autocomplete="name">
            @if (form.get('name')?.hasError('required')) {
              <mat-error>Name is required</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email">
            @if (form.get('email')?.hasError('required')) {
              <mat-error>Email is required</mat-error>
            }
            @if (form.get('email')?.hasError('email')) {
              <mat-error>Please enter a valid email</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Phone (optional)</mat-label>
            <input matInput formControlName="phoneNumber" autocomplete="tel">
          </mat-form-field>

          <mat-checkbox formControlName="receiveJobMatchNotifications" class="register-check">
            Email me when a new position matches my CV
          </mat-checkbox>

          <mat-checkbox formControlName="hasAcceptedTerms" class="register-check">
            I agree that Hunter stores my CV and contact details so people inside the platform can refer me.
            I can ask for them to be deleted at any time.
          </mat-checkbox>
          @if (form.get('hasAcceptedTerms')?.touched && form.get('hasAcceptedTerms')?.invalid) {
            <div class="register-error">You need to accept this to continue.</div>
          }

          @if (errorMessage) {
            <div class="register-error">
              <mat-icon>error_outline</mat-icon>
              {{ errorMessage }}
            </div>
          }
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="dialogRef.close()" [disabled]="saving">Cancel</button>
        <button mat-flat-button color="primary" type="submit" form="public-register-form" [disabled]="saving">
          @if (saving) {
            <mat-spinner diameter="18" class="button-spinner"></mat-spinner>
          } @else {
            <span>{{ data.reason === 'referral' ? 'Continue' : 'Save and send confirmation' }}</span>
          }
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .register-dialog {
      min-width: min(520px, 92vw);
      padding: var(--hunter-space-2);
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: var(--hunter-space-2);

      mat-icon { color: var(--hunter-brand); }
    }

    .register-intro {
      color: var(--hunter-text-secondary);
      margin: 0 0 var(--hunter-space-4);
    }

    .full-width { width: 100%; }

    .register-check {
      display: block;
      margin-bottom: var(--hunter-space-2);
      font-size: 0.875rem;
    }

    .register-error {
      display: flex;
      align-items: center;
      gap: var(--hunter-space-2);
      margin-top: var(--hunter-space-2);
      padding: var(--hunter-space-2) var(--hunter-space-3);
      border-radius: var(--hunter-radius-sm);
      background: var(--hunter-error-bg);
      color: var(--hunter-error-fg);
      font-size: 0.8125rem;
    }

    .button-spinner { display: inline-block; }
  `],
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicRegisterDialogComponent {
  form: FormGroup;
  saving = false;
  errorMessage = '';

  constructor(
    public dialogRef: MatDialogRef<PublicRegisterDialogComponent, PublicRegisterDialogResult>,
    @Inject(MAT_DIALOG_DATA) public data: PublicRegisterDialogData,
    private fb: FormBuilder,
    private publicService: PublicService
  ) {
    this.form = this.fb.group({
      name: [data.prefill?.name ?? '', Validators.required],
      email: [data.prefill?.email ?? '', [Validators.required, Validators.email]],
      phoneNumber: [data.prefill?.phoneNumber ?? ''],
      receiveJobMatchNotifications: [data.prefill?.receiveJobMatchNotifications ?? true],
      hasAcceptedTerms: [data.prefill?.hasAcceptedTerms ?? false, Validators.requiredTrue]
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving) {
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    const profile = this.form.getRawValue() as RegisterPublicCandidateRequest;

    this.publicService.register(this.data.candidateToken, profile).subscribe({
      next: (response) => {
        this.saving = false;
        this.dialogRef.close({ profile, response });
      },
      error: (error) => {
        this.saving = false;
        this.errorMessage = extractApiError(error, 'We could not save your profile. Please try again.');
      }
    });
  }
}
