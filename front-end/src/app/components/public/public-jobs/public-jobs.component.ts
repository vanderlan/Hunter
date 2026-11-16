import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { PublicJobPosition } from '../../../models/public.model';
import { PublicService } from '../../../services/public.service';

@Component({
  selector: 'app-public-jobs',
  imports: [
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './public-jobs.component.html',
  styleUrl: './public-jobs.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicJobsComponent implements OnInit, OnDestroy {
  positions: PublicJobPosition[] = [];
  filteredPositions: PublicJobPosition[] = [];
  loading = false;
  loadError = false;
  searchQuery = '';

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(private publicService: PublicService) {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => this.filterPositions());
  }

  ngOnInit(): void {
    this.loadPositions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPositions(): void {
    this.loading = true;
    this.loadError = false;
    this.publicService.getJobPositions().subscribe({
      next: (response) => {
        this.positions = response.jobs;
        this.filteredPositions = this.positions;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.loadError = true;
      }
    });
  }

  onSearchChange(term: string): void {
    this.searchQuery = term;
    this.searchSubject.next(term);
  }

  filterPositions(): void {
    const term = this.searchQuery.trim().toLowerCase();
    if (!term) {
      this.filteredPositions = this.positions;
      return;
    }
    this.filteredPositions = this.positions.filter(p =>
      p.title.toLowerCase().includes(term) ||
      (p.summary ?? '').toLowerCase().includes(term) ||
      (p.mustHaveSkills ?? '').toLowerCase().includes(term) ||
      (p.niceToHaveSkills ?? '').toLowerCase().includes(term)
    );
  }

  skills(position: PublicJobPosition, max = 5): string[] {
    return (position.mustHaveSkills ?? '')
      .split(/[,;\n]/)
      .map(s => s.trim())
      .filter(s => s.length > 0)
      .slice(0, max);
  }

  trackByCode(_: number, position: PublicJobPosition): string {
    return position.code;
  }
}
