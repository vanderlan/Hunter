import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'hunter_theme';
  private isDarkTheme = new BehaviorSubject<boolean>(false);
  public isDarkTheme$ = this.isDarkTheme.asObservable();

  constructor() {
    const savedTheme = this.getStoredTheme();
    this.isDarkTheme.next(savedTheme === 'dark');
    this.applyTheme(savedTheme === 'dark');
  }

  toggleTheme(): void {
    const newTheme = !this.isDarkTheme.value;
    this.isDarkTheme.next(newTheme);
    this.applyTheme(newTheme);
    this.storeTheme(newTheme ? 'dark' : 'light');
  }

  private applyTheme(isDark: boolean): void {
    if (typeof document !== 'undefined') {
      if (isDark) {
        document.body.classList.add('dark-theme');
        document.body.classList.remove('light-theme');
      } else {
        document.body.classList.add('light-theme');
        document.body.classList.remove('dark-theme');
      }
    }
  }

  private getStoredTheme(): string {
    if (typeof window !== 'undefined') {
      return localStorage.getItem(this.THEME_KEY) || 'light';
    }
    return 'light';
  }

  private storeTheme(theme: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.THEME_KEY, theme);
    }
  }
}
