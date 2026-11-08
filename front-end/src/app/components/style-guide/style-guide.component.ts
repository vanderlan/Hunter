import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotificationService } from '../../services/notification.service';
import { ThemeService } from '../../services/theme.service';

interface TokenSwatch {
  name: string;
  token: string;
}

/**
 * Living style guide for the Hunter design language.
 *
 * This page renders from the same tokens everything else uses, so it cannot
 * drift: if a token changes, this page changes with it. Use it to check a new
 * screen against the system before shipping, and to verify both colour
 * schemes.
 */
@Component({
  selector: 'app-style-guide',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatCheckboxModule, MatSlideToggleModule,
    MatChipsModule, MatTabsModule, MatTooltipModule,
  ],
  templateUrl: './style-guide.component.html',
  styleUrl: './style-guide.component.scss',
})
export class StyleGuideComponent {
  private readonly notifications = inject(NotificationService);
  readonly theme = inject(ThemeService);

  readonly surfaces: TokenSwatch[] = [
    { name: 'bg', token: '--hunter-bg' },
    { name: 'surface', token: '--hunter-surface' },
    { name: 'surface-variant', token: '--hunter-surface-variant' },
    { name: 'surface-hover', token: '--hunter-surface-hover' },
    { name: 'surface-raised', token: '--hunter-surface-raised' },
    { name: 'surface-sunken', token: '--hunter-surface-sunken' },
  ];

  readonly brand: TokenSwatch[] = [
    { name: 'brand', token: '--hunter-brand' },
    { name: 'brand-container', token: '--hunter-brand-container' },
    { name: 'brand-tint', token: '--hunter-brand-tint' },
    { name: 'accent', token: '--hunter-accent' },
    { name: 'accent-container', token: '--hunter-accent-container' },
  ];

  readonly statuses = [
    'success', 'error', 'warn', 'info', 'teal', 'purple', 'indigo', 'yellow',
  ] as const;

  /** The 8 pipeline stages, in journey order (cool -> warm). */
  readonly stages = [
    { key: 'new', label: 'New', css: 'status-new' },
    { key: 'analysis', label: 'Pending Analysis', css: 'status-referred-partner' },
    { key: 'internal', label: 'Referred Internally', css: 'status-referred-internal' },
    { key: 'screening', label: 'Screening', css: 'status-screening' },
    { key: 'technical', label: 'Tech Interview', css: 'status-technical' },
    { key: 'client', label: 'Client Interview', css: 'status-client' },
    { key: 'hired', label: 'Hired', css: 'status-hired' },
    { key: 'reproved', label: 'Reproved', css: 'status-reproved' },
  ];

  readonly vizSeries = [1, 2, 3, 4, 5, 6];
  readonly scoreStops = [0, 25, 50, 75, 100];

  readonly spacing = [1, 2, 3, 4, 5, 6, 8, 10, 12, 16];
  readonly radii = ['xs', 'sm', 'md', 'lg', 'xl', 'pill'];
  readonly density = [
    { token: '--hunter-row-height', label: 'Table row' },
    { token: '--hunter-control-height', label: 'Control' },
    { token: '--hunter-topbar-height', label: 'Top bar' },
    { token: '--hunter-sidenav-width', label: 'Sidebar' },
  ];
  readonly shadows = ['sm', 'md', 'lg', 'xl'];

  readonly typeScale = [
    { label: 'Display', cls: 'type-display', sample: 'Hunter' },
    { label: 'Heading 1', cls: 'type-h1', sample: 'Candidate matches' },
    { label: 'Heading 2', cls: 'type-h2', sample: 'Open positions' },
    { label: 'Heading 3', cls: 'type-h3', sample: 'Hiring process' },
    { label: 'Body', cls: 'type-body', sample: 'The quick brown fox jumps over the lazy dog.' },
    { label: 'Small', cls: 'type-small', sample: 'Secondary and supporting copy.' },
    { label: 'Caption', cls: 'type-caption', sample: 'LAST UPDATED 2 MINUTES AGO' },
  ];

  readonly lastToast = signal<string>('none yet');

  toast(tone: 'success' | 'error' | 'warning' | 'info'): void {
    const messages: Record<string, string> = {
      success: 'Candidate moved to Interview stage.',
      error: 'Could not reach the matching service. Try again.',
      warning: 'This position closes in 2 days.',
      info: 'Analysis finished — 14 candidates scored.',
    };
    this.notifications[tone](messages[tone]);
    this.lastToast.set(tone);
  }

  toggleTheme(): void {
    this.theme.toggleTheme();
  }
}
