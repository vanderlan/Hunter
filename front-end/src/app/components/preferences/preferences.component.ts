import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../services/user.service';

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
    private snackBar: MatSnackBar,
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
        this.snackBar.open('Error loading preferences', 'Close', { duration: 3000 });
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
        this.snackBar.open('Preferences saved successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.saving = false;
        this.snackBar.open('Error saving preferences', 'Close', { duration: 3000 });
      }
    });
  }
}
