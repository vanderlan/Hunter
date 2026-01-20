import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './preferences.component.html',
  styleUrl: './preferences.component.scss'
})
export class PreferencesComponent implements OnInit {
  preferencesForm: FormGroup;
  loading = false;
  saving = false;

  constructor(
    private userService: UserService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {
    this.preferencesForm = this.fb.group({
      receiveJobChangeNotifications: [true]
    });
  }

  ngOnInit(): void {
    this.loadPreferences();
  }

  loadPreferences(): void {
    this.loading = true;
    this.userService.getPreferences().subscribe({
      next: (preferences) => {
        this.preferencesForm.patchValue(preferences);
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading preferences');
      }
    });
  }

  savePreferences(): void {
    if (this.preferencesForm.invalid) {
      return;
    }

    this.saving = true;
    const preferences = this.preferencesForm.value;

    this.userService.updatePreferences(preferences).subscribe({
      next: () => {
        this.saving = false;
        this.notificationService.success('Preferences saved successfully');
      },
      error: (error) => {
        this.saving = false;
        this.notificationService.error('Error saving preferences');
      }
    });
  }
}
