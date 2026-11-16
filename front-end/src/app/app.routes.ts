import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./components/login/login.component').then(m => m.LoginComponent)
  },
  {
    // Anonymous surface: job board, CV match, email confirmation. Rendered in
    // its own shell (see AppComponent.isPublicRoute) with no sidenav.
    path: '',
    loadComponent: () => import('./components/public/public-layout/public-layout.component').then(m => m.PublicLayoutComponent),
    data: { layout: 'public' },
    children: [
      {
        path: '',
        loadComponent: () => import('./components/public/public-jobs/public-jobs.component').then(m => m.PublicJobsComponent)
      },
      {
        path: 'jobs/:code',
        loadComponent: () => import('./components/public/public-job-detail/public-job-detail.component').then(m => m.PublicJobDetailComponent)
      },
      {
        path: 'match',
        loadComponent: () => import('./components/public/public-cv-match/public-cv-match.component').then(m => m.PublicCvMatchComponent)
      },
      {
        path: 'confirm',
        loadComponent: () => import('./components/public/public-confirm-email/public-confirm-email.component').then(m => m.PublicConfirmEmailComponent)
      }
    ]
  },
  {
    path: 'analyze',
    loadComponent: () => import('./components/cv-analyzer/cv-analyzer.component').then(m => m.CvAnalyzerComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'referral-requests',
    loadComponent: () => import('./components/referral-requests/referral-requests.component').then(m => m.ReferralRequestsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'open-positions',
    loadComponent: () => import('./components/open-positions/open-positions.component').then(m => m.OpenPositionsComponent),
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
    path: 'ai-settings',
    loadComponent: () => import('./components/ai-settings/ai-settings.component').then(m => m.AiSettingsComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'looker-studio',
    loadComponent: () => import('./components/looker-studio-admin/looker-studio-admin.component').then(m => m.LookerStudioAdminComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'candidates',
    loadComponent: () => import('./components/candidates/candidates.component').then(m => m.CandidatesComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'hiring-process',
    loadComponent: () => import('./components/hiring-process/hiring-process.component').then(m => m.HiringProcessComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'hiring-process/:id',
    loadComponent: () => import('./components/hiring-process-detail/hiring-process-detail.component').then(m => m.HiringProcessDetailComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'change-password',
    loadComponent: () => import('./components/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'preferences',
    loadComponent: () => import('./components/preferences/preferences.component').then(m => m.PreferencesComponent),
    canActivate: [AuthGuard]
  },
  {
    // Living style guide for the Hunter design language. Admin-only: it is a
    // development reference, not a product surface.
    path: 'style-guide',
    loadComponent: () => import('./components/style-guide/style-guide.component').then(m => m.StyleGuideComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
