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
      height: 28px;
      padding: 4px 12px;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      
      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
        margin: 0;
      }
    }

    /* High Priority - Red/Fire Theme */
    .priority-high {
      background: linear-gradient(135deg, #ff5252 0%, #ff1744 100%);
      color: white !important;
      box-shadow: 0 2px 4px rgba(255, 23, 68, 0.3);
      
      mat-icon {
        animation: pulse 2s ease-in-out infinite;
      }
    }

    /* Medium Priority - Green Theme */
    .priority-medium {
      background: linear-gradient(135deg, #66bb6a 0%, #43a047 100%);
      color: white !important;
      box-shadow: 0 2px 4px rgba(67, 160, 71, 0.3);
    }

    /* Low Priority - Light Blue Theme */
    .priority-low {
      background: linear-gradient(135deg, #42a5f5 0%, #1e88e5 100%);
      color: white !important;
      box-shadow: 0 2px 4px rgba(30, 136, 229, 0.3);
    }

    /* No Priority - Gray Theme */
    .priority-none {
      background: #e0e0e0;
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
    if (priorityLower === 'high') return 'High Priority - Hot opportunity!';
    if (priorityLower === 'medium') return 'Medium Priority - Good opportunity';
    if (priorityLower === 'low') return 'Low Priority - Consider if interested';
    return 'Priority not set';
  }
}
