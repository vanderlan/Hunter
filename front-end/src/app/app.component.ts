import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, Router, NavigationEnd, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  isAuthenticated$ = this.authService.isAuthenticated$;
  sidenavExpanded = true;
  currentRoute = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Track current route for page title
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentRoute = event.url;
    });
  }

  toggleSidenav(): void {
    this.sidenavExpanded = !this.sidenavExpanded;
    // Save preference to localStorage
    localStorage.setItem('sidenavExpanded', JSON.stringify(this.sidenavExpanded));
  }

  ngOnInit(): void {
    // Restore sidenav state from localStorage
    const savedState = localStorage.getItem('sidenavExpanded');
    if (savedState !== null) {
      this.sidenavExpanded = JSON.parse(savedState);
    }
  }

  logout(): void {
    this.authService.logout();
  }

  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  getUserName(): string {
    return this.authService.getUserName() || 'User';
  }

  changePassword(): void {
    this.router.navigate(['/change-password']);
  }

  preferences(): void {
    this.router.navigate(['/preferences']);
  }

  getPageTitle(): string {
    const route = this.currentRoute;
    if (route === '/' || route === '') return 'Analyze CV';
    if (route.includes('/open-positions')) return 'Open Positions';
    if (route.includes('/hiring-process')) return this.getReferralsLabel();
    if (route.includes('/candidates')) return 'Candidates';
    if (route.includes('/admin')) return 'Admin Panel';
    if (route.includes('/job-positions')) return 'Job Positions';
    if (route.includes('/users')) return 'Users';
    if (route.includes('/change-password')) return 'Change Password';
    if (route.includes('/preferences')) return 'Preferences';
    return 'Hunter';
  }

  getReferralsLabel(): string {
    return this.isAdmin() ? 'Referrals' : 'My Referrals';
  }
}
