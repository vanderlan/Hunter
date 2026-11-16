import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PublicCvMatchResponse, PublicJobMatch, PublicMatchSession } from '../../../models/public.model';
import { NotificationService } from '../../../services/notification.service';
import { PublicService } from '../../../services/public.service';
import { extractApiError } from '../../../utils/api-error';
import { PublicRegisterDialogComponent, PublicRegisterDialogResult } from './public-register-dialog.component';
import { PublicReferralDialogComponent } from './public-referral-dialog.component';

type MatchState = 'idle' | 'analyzing' | 'results';

@Component({
  selector: 'app-public-cv-match',
  imports: [
    DecimalPipe,
    RouterLink,
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatExpansionModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './public-cv-match.component.html',
  styleUrl: './public-cv-match.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicCvMatchComponent implements OnInit {
  state: MatchState = 'idle';
  dragOver = false;
  selectedFile: File | null = null;
  errorMessage = '';
  session: PublicMatchSession | null = null;
  /** Job code passed from a job detail page; its match is highlighted. */
  highlightJobCode: string | null = null;
  resendingConfirmation = false;

  constructor(
    private publicService: PublicService,
    private notificationService: NotificationService,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.highlightJobCode = this.route.snapshot.queryParamMap.get('job');
    this.session = this.publicService.loadSession();
    if (this.session) {
      this.state = 'results';
    }
  }

  // ---- file selection -------------------------------------------------------

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(): void {
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.pickFile(file);
    }
  }

  onFileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.pickFile(file);
    }
    input.value = '';
  }

  private pickFile(file: File): void {
    this.errorMessage = '';
    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      this.errorMessage = 'Please upload your CV as a PDF file.';
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this.errorMessage = 'The file is larger than 10MB.';
      return;
    }
    this.selectedFile = file;
  }

  clearFile(): void {
    this.selectedFile = null;
    this.errorMessage = '';
  }

  // ---- analysis -------------------------------------------------------------

  analyze(): void {
    if (!this.selectedFile || this.state === 'analyzing') {
      return;
    }

    this.state = 'analyzing';
    this.errorMessage = '';

    this.publicService.matchCv(this.selectedFile).subscribe({
      next: (result) => this.showResults(result),
      error: (error) => {
        this.state = 'idle';
        this.errorMessage = this.extractError(error, 'We could not analyze your CV right now. Please try again in a moment.');
      }
    });
  }

  private showResults(result: PublicCvMatchResponse): void {
    this.session = {
      result,
      isEmailConfirmed: false,
      requestedJobCodes: []
    };
    this.publicService.saveSession(this.session);
    this.state = 'results';
    this.selectedFile = null;
  }

  startOver(): void {
    this.publicService.clearSession();
    this.session = null;
    this.state = 'idle';
    this.selectedFile = null;
    this.errorMessage = '';
  }

  // ---- profile --------------------------------------------------------------

  get hasProfile(): boolean {
    return !!this.session?.profile;
  }

  get isHighlighted(): (match: PublicJobMatch) => boolean {
    return (match) => !!this.highlightJobCode && match.jobCode === this.highlightJobCode;
  }

  openRegister(reason: 'save' | 'referral' = 'save'): Promise<PublicRegisterDialogResult | undefined> {
    const session = this.session!;
    const dialogRef = this.dialog.open(PublicRegisterDialogComponent, {
      data: {
        candidateToken: session.result.candidateToken,
        reason,
        prefill: session.profile ?? {
          name: session.result.extractedName ?? '',
          email: session.result.extractedEmail ?? '',
          phoneNumber: session.result.extractedPhone ?? ''
        }
      },
      disableClose: true,
      width: '560px',
      maxWidth: '95vw'
    });

    return new Promise(resolve => {
      dialogRef.afterClosed().subscribe((result?: PublicRegisterDialogResult) => {
        if (result && this.session) {
          this.session.profile = result.profile;
          this.session.isEmailConfirmed = result.response.isEmailConfirmed;
          this.publicService.saveSession(this.session);

          if (result.response.isEmailConfirmed) {
            this.notificationService.success('Profile saved.');
          } else if (result.response.isConfirmationEmailSent) {
            this.notificationService.success(`We sent a confirmation link to ${result.response.email}.`);
          } else {
            this.notificationService.warning('Profile saved, but we could not send the confirmation email. Use "Resend" to try again.');
          }
        }
        resolve(result);
      });
    });
  }

  resendConfirmation(): void {
    if (!this.session?.profile || this.resendingConfirmation) {
      return;
    }
    this.resendingConfirmation = true;
    this.publicService.register(this.session.result.candidateToken!, this.session.profile).subscribe({
      next: (response) => {
        this.resendingConfirmation = false;
        if (response.isEmailConfirmed) {
          this.session!.isEmailConfirmed = true;
          this.publicService.saveSession(this.session!);
          this.notificationService.success('Your email is already confirmed.');
        } else if (response.isConfirmationEmailSent) {
          this.notificationService.success(`Confirmation link sent again to ${response.email}.`);
        } else {
          this.notificationService.error('We could not send the email. Please try again later.');
        }
      },
      error: (error) => {
        this.resendingConfirmation = false;
        this.notificationService.error(this.extractError(error, 'We could not resend the confirmation email.'));
      }
    });
  }

  // ---- referral -------------------------------------------------------------

  isRequested(match: PublicJobMatch): boolean {
    return !!this.session?.requestedJobCodes.includes(match.jobCode);
  }

  referralTooltip(match: PublicJobMatch): string {
    if (this.isRequested(match)) {
      return 'You already asked for a referral for this position';
    }
    if (!match.canRequestReferral) {
      return `Referrals open for matches of ${this.session?.result.minimumMatchForReferral ?? 50}% or more`;
    }
    return '';
  }

  async askForReferral(match: PublicJobMatch): Promise<void> {
    if (!this.session?.result.candidateToken || !match.canRequestReferral || this.isRequested(match)) {
      return;
    }

    if (!this.hasProfile) {
      const registered = await this.openRegister('referral');
      if (!registered) {
        return;
      }
    }

    const dialogRef = this.dialog.open(PublicReferralDialogComponent, {
      data: {
        candidateToken: this.session.result.candidateToken,
        match,
        isEmailConfirmed: this.session.isEmailConfirmed
      },
      width: '560px',
      maxWidth: '95vw'
    });

    dialogRef.afterClosed().subscribe(response => {
      if (response && this.session) {
        this.session.requestedJobCodes = [...this.session.requestedJobCodes, match.jobCode];
        this.publicService.saveSession(this.session);
        this.notificationService.success(
          response.isEmailConfirmationRequired
            ? 'Request saved. Confirm your email so we can deliver it.'
            : 'Request sent. Someone inside will take a look.'
        );
      }
    });
  }

  // ---- helpers --------------------------------------------------------------

  matchClass(percentage: number): string {
    if (percentage >= 80) return 'match-high';
    if (percentage >= 60) return 'match-medium';
    return 'match-low';
  }

  private extractError(error: any, fallback: string): string {
    return extractApiError(error, fallback);
  }
}
