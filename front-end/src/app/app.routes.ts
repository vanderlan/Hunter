import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./components/cv-analyzer/cv-analyzer.component').then(m => m.CvAnalyzerComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'admin',
    loadComponent: () => import('./components/admin/admin.component').then(m => m.AdminComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'job-positions',
    loadComponent: () => import('./components/job-positions/job-positions.component').then(m => m.JobPositionsComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'users',
    loadComponent: () => import('./components/user-management/user-management.component').then(m => m.UserManagementComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'candidates',
    loadComponent: () => import('./components/candidates/candidates.component').then(m => m.CandidatesComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'change-password',
    loadComponent: () => import('./components/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [AuthGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
