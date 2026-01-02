import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';

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
    path: 'job-positions',
    loadComponent: () => import('./components/job-positions/job-positions.component').then(m => m.JobPositionsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
