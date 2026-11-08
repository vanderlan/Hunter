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
    // The whole theme resolves from the CSS `color-scheme` property: Material's
    // system tokens and our own are emitted as light-dark() pairs, so toggling
    // this one class repaints both without shipping a second stylesheet.
    document.documentElement.classList.toggle('hunter-light', !isDark);

    // Kept in sync for any consumer that still keys off the body class.
    document.body.classList.toggle('dark-theme', isDark);
    document.body.classList.toggle('light-theme', !isDark);

    document
      .querySelector('meta[name="theme-color"]')
      ?.setAttribute('content', isDark ? '#141110' : '#fff8f6');
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
