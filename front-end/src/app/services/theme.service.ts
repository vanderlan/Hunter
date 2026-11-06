import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type HunterTheme = 'dark' | 'light';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'hunter_theme';
  private readonly DEFAULT_THEME: HunterTheme = 'dark';

  private isDarkTheme = new BehaviorSubject<boolean>(true);
  public isDarkTheme$ = this.isDarkTheme.asObservable();

  constructor() {
    const theme = this.getStoredTheme();
    this.isDarkTheme.next(theme === 'dark');
    this.applyTheme(theme === 'dark');
  }

  get isDark(): boolean {
    return this.isDarkTheme.value;
  }

  toggleTheme(): void {
    this.setTheme(this.isDarkTheme.value ? 'light' : 'dark');
  }

  setTheme(theme: HunterTheme): void {
    const isDark = theme === 'dark';
    this.isDarkTheme.next(isDark);
    this.applyTheme(isDark);
    this.storeTheme(theme);
  }

  private applyTheme(isDark: boolean): void {
    if (typeof document === 'undefined') {
      return;
    }
    // Dark is the baseline (tokens live on <body>); light-theme is the opt-in override.
    document.body.classList.toggle('dark-theme', isDark);
    document.body.classList.toggle('light-theme', !isDark);
  }

  private getStoredTheme(): HunterTheme {
    if (typeof window === 'undefined') {
      return this.DEFAULT_THEME;
    }
    const stored = localStorage.getItem(this.THEME_KEY);
    return stored === 'light' || stored === 'dark' ? stored : this.DEFAULT_THEME;
  }

  private storeTheme(theme: HunterTheme): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.THEME_KEY, theme);
    }
  }
}
