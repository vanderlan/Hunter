import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';
import { User, UserProfile, UserCreateRequest, UserUpdateRequest } from '../../models/user.model';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatTooltipModule,
    MatCheckboxModule
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent implements OnInit {
  displayedColumns: string[] = ['name', 'email', 'profile', 'createdAt', 'actions'];
  dataSource: User[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchQuery = '';
  loading = false;
  isEditing = false;
  userForm: FormGroup;
  currentUserId: string | null = null;

  userProfiles = [
    { value: UserProfile.Admin, label: 'Admin' },
    { value: UserProfile.Partner, label: 'Partner' }
  ];

  constructor(
    private userService: UserService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      profile: [UserProfile.Partner, Validators.required],
      sendWelcomeEmail: [true],
      receiveJobChangeNotifications: [true]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getUsers(this.pageIndex + 1, this.pageSize, this.searchQuery).subscribe({
      next: (response) => {
        this.dataSource = response.items;
        this.totalCount = response.count;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.error('Error loading users');
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadUsers();
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadUsers();
  }

  showCreateForm(): void {
    this.isEditing = true;
    this.currentUserId = null;
    this.userForm.reset({ profile: UserProfile.Partner, sendWelcomeEmail: true, receiveJobChangeNotifications: true });
    // Add password field for create
    if (!this.userForm.get('password')) {
      this.userForm.addControl('password', this.fb.control('', [Validators.required, Validators.minLength(3)]));
    }
    // Ensure sendWelcomeEmail control exists
    if (!this.userForm.get('sendWelcomeEmail')) {
      this.userForm.addControl('sendWelcomeEmail', this.fb.control(true));
    }
    // Ensure receiveJobChangeNotifications control exists
    if (!this.userForm.get('receiveJobChangeNotifications')) {
      this.userForm.addControl('receiveJobChangeNotifications', this.fb.control(true));
    }
  }

  editUser(user: User): void {
    this.isEditing = true;
    this.currentUserId = user.publicId!;
    // Remove password field for edit
    if (this.userForm.get('password')) {
      this.userForm.removeControl('password');
    }
    // Remove sendWelcomeEmail for edit (only for create)
    if (this.userForm.get('sendWelcomeEmail')) {
      this.userForm.removeControl('sendWelcomeEmail');
    }
    // Ensure receiveJobChangeNotifications control exists
    if (!this.userForm.get('receiveJobChangeNotifications')) {
      this.userForm.addControl('receiveJobChangeNotifications', this.fb.control(true));
    }
    this.userForm.patchValue({
      name: user.name,
      email: user.email,
      profile: user.profile,
      receiveJobChangeNotifications: user.receiveJobChangeNotifications ?? true
    });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.currentUserId = null;
    this.userForm.reset({ profile: UserProfile.Partner, sendWelcomeEmail: true, receiveJobChangeNotifications: true });
    if (this.userForm.get('password')) {
      this.userForm.removeControl('password');
    }
    if (this.userForm.get('sendWelcomeEmail')) {
      this.userForm.removeControl('sendWelcomeEmail');
    }
  }

  saveUser(): void {
    if (this.userForm.invalid) {
      this.notificationService.warning('Please fill in all required fields');
      return;
    }

    this.loading = true;

    if (this.currentUserId) {
      // Update
      const userData: UserUpdateRequest = this.userForm.value;
      this.userService.updateUser(this.currentUserId, userData).subscribe({
        next: () => {
          this.loading = false;
          this.notificationService.success('User updated successfully');
          this.cancelEdit();
          this.loadUsers();
        },
        error: (error) => {
          this.loading = false;
          this.notificationService.error(error.error?.message || 'Error updating user');
        }
      });
    } else {
      // Create
      const userData: UserCreateRequest = this.userForm.value;
      this.userService.createUser(userData).subscribe({
        next: () => {
          this.loading = false;
          this.notificationService.success('User created successfully');
          this.cancelEdit();
          this.loadUsers();
        },
        error: (error) => {
          this.loading = false;
          this.notificationService.error(error.error?.message || 'Error creating user');
        }
      });
    }
  }

  deleteUser(user: User): void {
    if (confirm(`Are you sure you want to delete user "${user.name}"?`)) {
      this.loading = true;
      this.userService.deleteUser(user.publicId!).subscribe({
        next: () => {
          this.loading = false;
          this.notificationService.success('User deleted successfully');
          this.loadUsers();
        },
        error: (error) => {
          this.loading = false;
          this.notificationService.error(error.error?.message || 'Error deleting user');
        }
      });
    }
  }

  getProfileLabel(profile: UserProfile): string {
    return profile === UserProfile.Admin ? 'Admin' : 'Partner';
  }
}
