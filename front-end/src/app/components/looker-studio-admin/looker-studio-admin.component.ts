import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { LookerStudioService, CredentialStatusResponse, StoreCredentialsResponse, SyncPrioritiesResponse } from '../../services/looker-studio.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-looker-studio-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatExpansionModule,
    MatChipsModule,
    MatDividerModule
  ],
  templateUrl: './looker-studio-admin.component.html',
  styleUrl: './looker-studio-admin.component.scss'
})
export class LookerStudioAdminComponent implements OnInit {
  curlCommand = '';
  expiresInDays = 7;
  isStoringCredentials = false;
  isSyncingPriorities = false;
  isLoadingStatus = false;
  
  credentialStatus?: CredentialStatusResponse;
  lastStoreResult?: StoreCredentialsResponse;
  lastSyncResult?: SyncPrioritiesResponse;
  
  showHowTo = false;

  constructor(
    private lookerStudioService: LookerStudioService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadCredentialStatus();
  }

  loadCredentialStatus(): void {
    this.isLoadingStatus = true;
    this.lookerStudioService.getCredentialStatus().subscribe({
      next: (status) => {
        this.credentialStatus = status;
        this.isLoadingStatus = false;
      },
      error: (error) => {
        this.isLoadingStatus = false;
        console.error('Error loading credential status', error);
      }
    });
  }

  storeCredentials(): void {
    if (!this.curlCommand.trim()) {
      this.notificationService.error('Please paste the curl command');
      return;
    }

    this.isStoringCredentials = true;
    this.lastStoreResult = undefined;

    this.lookerStudioService.storeCredentials(this.curlCommand, this.expiresInDays).subscribe({
      next: (result) => {
        this.isStoringCredentials = false;
        this.lastStoreResult = result;
        
        if (result.isValid) {
          this.notificationService.success('Credentials stored and validated successfully!');
          this.curlCommand = ''; // Clear the input
        } else {
          this.notificationService.warning('Credentials stored but validation failed. They may be expired.');
        }
        
        this.loadCredentialStatus();
      },
      error: (error) => {
        this.isStoringCredentials = false;
        const errorMessage = error.error?.message || 'Error storing credentials. Please check the curl command and try again.';
        this.notificationService.error(errorMessage);
      }
    });
  }

  syncPriorities(): void {
    if (!this.credentialStatus?.hasActiveCredential) {
      this.notificationService.error('Please store credentials first');
      return;
    }

    if (this.credentialStatus.isExpired) {
      this.notificationService.error('Credentials have expired. Please update them first.');
      return;
    }

    this.isSyncingPriorities = true;
    this.lastSyncResult = undefined;

    this.lookerStudioService.syncPriorities().subscribe({
      next: (result) => {
        this.isSyncingPriorities = false;
        this.lastSyncResult = result;
        
        const message = `Sync completed: ${result.matchedJobs} matched, ${result.updatedJobs} updated`;
        this.notificationService.success(message);
        
        this.loadCredentialStatus();
      },
      error: (error) => {
        this.isSyncingPriorities = false;
        const errorMessage = error.error?.message || 'Error syncing priorities. Please try again.';
        this.notificationService.error(errorMessage);
      }
    });
  }

  getStatusClass(): string {
    if (!this.credentialStatus?.hasActiveCredential) return 'status-none';
    if (this.credentialStatus.isExpired) return 'status-expired';
    if ((this.credentialStatus.daysUntilExpiration ?? 0) <= 2) return 'status-warning';
    return 'status-active';
  }

  getStatusIcon(): string {
    if (!this.credentialStatus?.hasActiveCredential) return 'error';
    if (this.credentialStatus.isExpired) return 'dangerous';
    if ((this.credentialStatus.daysUntilExpiration ?? 0) <= 2) return 'warning';
    return 'check_circle';
  }
}
