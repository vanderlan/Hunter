import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject } from 'rxjs';
import { switchMap, takeUntil } from 'rxjs/operators';
import { PublicJobPositionDetail } from '../../../models/public.model';
import { PublicService } from '../../../services/public.service';

@Component({
  selector: 'app-public-job-detail',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './public-job-detail.component.html',
  styleUrl: './public-job-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager
})
export class PublicJobDetailComponent implements OnInit, OnDestroy {
  position: PublicJobPositionDetail | null = null;
  loading = true;
  notFound = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private publicService: PublicService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        this.loading = true;
        this.notFound = false;
        this.position = null;
        return this.publicService.getJobPosition(params.get('code') ?? '');
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (position) => {
        this.position = position;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notFound = true;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  splitList(value?: string): string[] {
    return (value ?? '')
      .split(/[,;\n]/)
      .map(s => s.trim())
      .filter(s => s.length > 0);
  }
}
