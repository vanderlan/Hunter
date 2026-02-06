import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  imports: [CommonModule, MatChipsModule, MatIconModule, MatTooltipModule],
  template: `
    @if (priority) {
      <mat-chip [class]="getPriorityClass()" [matTooltip]="getTooltip()">
        <mat-icon [class]="getIconClass()">{{ getIcon() }}</mat-icon>
        <span>{{ priority }}</span>
      </mat-chip>
    }
  `,
  styles: [`
    mat-chip {
      font-weight: 500;
      font-size: 0.875rem;
      height: 32px;
      padding: 6px 12px;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      border-radius: 16px;
      
      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
        line-height: 18px;
        margin: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        overflow: visible;
      }
    }

    /* Override Material's default chip background */
    ::ng-deep .mat-mdc-standard-chip:not(.mdc-evolution-chip--disabled) {
      background-color: transparent !important;
    }

    /* High Priority - Red/Fire Theme */
    .priority-high {
      background: linear-gradient(135deg, #e96161 0%, #e96161 100%) !important;
      color: white !important;
      box-shadow: 0 2px 4px rgba(255, 23, 68, 0.3);
      
      mat-icon {
        animation: pulse 2s ease-in-out infinite;
      }
    }

    /* Medium Priority - Green Theme */
    .priority-medium {
      background: linear-gradient(135deg, #77b47a 0%, #77b47a 100%) !important;
      color: white !important;
      box-shadow: 0 2px 4px rgba(67, 160, 71, 0.3);
    }

    /* Low Priority - Light Blue Theme */
    .priority-low {
      background: linear-gradient(135deg, #79a8ce 0%, #79a8ce 100%) !important;
      color: white !important;
      box-shadow: 0 2px 4px rgba(30, 136, 229, 0.3);
    }

    /* No Priority - Gray Theme */
    .priority-none {
      background: #e0e0e0 !important;
      color: #757575 !important;
    }

    /* Pulse animation for high priority */
    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
        opacity: 1;
      }
      50% {
        transform: scale(1.1);
        opacity: 0.8;
      }
    }

    /* Icon animations */
    .icon-high {
      filter: drop-shadow(0 0 4px rgba(255, 255, 255, 0.5));
    }
    
    /* Override Material defaults */
    ::ng-deep .mat-mdc-chip {
      overflow: visible !important;
    }
    
    ::ng-deep .mat-mdc-chip-action-label {
      overflow: visible !important;
      display: flex !important;
      align-items: center !important;
      gap: 6px !important;
    }
  `]
})
export class PriorityBadgeComponent {
  @Input() priority?: string;

  getPriorityClass(): string {
    if (!this.priority) return 'priority-none';
    
    const priorityLower = this.priority.toLowerCase();
    if (priorityLower === 'high') return 'priority-high';
    if (priorityLower === 'medium') return 'priority-medium';
    if (priorityLower === 'low') return 'priority-low';
    return 'priority-none';
  }

  getIcon(): string {
    if (!this.priority) return 'remove';
    
    const priorityLower = this.priority.toLowerCase();
    if (priorityLower === 'high') return 'local_fire_department';
    if (priorityLower === 'medium') return 'check_circle';
    if (priorityLower === 'low') return 'info';
    return 'remove';
  }

  getIconClass(): string {
    if (!this.priority) return '';
    
    const priorityLower = this.priority.toLowerCase();
    if (priorityLower === 'high') return 'icon-high';
    return '';
  }

  getTooltip(): string {
    if (!this.priority) return 'No priority set';
    
    const priorityLower = this.priority.toLowerCase();
    if (priorityLower === 'high') return 'High Priority';
    if (priorityLower === 'medium') return 'Medium Priority';
    if (priorityLower === 'low') return 'Low Priority';
    return 'Priority not set';
  }
}
