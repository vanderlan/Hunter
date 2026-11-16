import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../services/auth.service';
import { ThemeService } from '../../../services/theme.service';

/**
 * Shell for the anonymous pages: job board, CV match and email confirmation.
 * No sidenav, no user menu; just a top bar with a way into the app.
 */
@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicLayoutComponent {
  readonly year = new Date().getFullYear();

  constructor(
    private authService: AuthService,
    private themeService: ThemeService
  ) {}

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  isDarkTheme(): boolean {
    return this.themeService.isDark;
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
