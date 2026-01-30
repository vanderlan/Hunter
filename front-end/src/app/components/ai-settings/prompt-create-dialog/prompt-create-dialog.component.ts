import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { QuillModule } from 'ngx-quill';
import { PromptCreateRequest } from '../../../services/prompt.service';

@Component({
  selector: 'app-prompt-create-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    QuillModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>add</mat-icon>
      Create New Prompt
    </h2>

    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Key</mat-label>
        <input matInput [(ngModel)]="key" placeholder="PROMPT_KEY" (input)="onKeyInput()">
        <mat-hint>Use UPPERCASE with underscores (e.g., MY_PROMPT)</mat-hint>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Name</mat-label>
        <input matInput [(ngModel)]="name" placeholder="Prompt display name" maxlength="200">
        <mat-hint>Maximum 200 characters</mat-hint>
      </mat-form-field>

      <div class="prompt-editor-wrapper">
        <label class="editor-label">Prompt Text</label>
        <quill-editor 
          [(ngModel)]="text"
          [modules]="quillModules"
          [styles]="{ height: '300px' }"
          placeholder="Enter your prompt text here..."
          class="quill-editor">
        </quill-editor>
        <div class="editor-hint">
          Use placeholders like &#123;jobTitle&#125;, &#123;rawHtmlContent&#125;, &#123;cvText&#125;, &#123;jobPositions&#125;, &#123;minimumMatchPercentage&#125;
        </div>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onCreate()" [disabled]="!isValid()">
        <mat-icon>add</mat-icon>
        Create
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 20px;
    }

    mat-dialog-content {
      min-width: 700px;
      max-height: 80vh;
      overflow-y: auto;
      padding: 24px;
    }

    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 24px 0;
      padding: 0 24px;
      font-size: 20px;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.87);
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin: 0;
      border-top: 1px solid #e0e0e0;
      background-color: #fafafa;
    }

    button[mat-raised-button] {
      min-width: 100px;
    }

    .prompt-editor-wrapper {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-bottom: 20px;

      .editor-label {
        font-size: 14px;
        font-weight: 500;
        color: rgba(0, 0, 0, 0.87);
      }

      .quill-editor {
        border: 1px solid rgba(0, 0, 0, 0.12);
        border-radius: 4px;
        background: white;

        ::ng-deep .ql-container {
          font-family: 'SF Mono', Monaco, Menlo, Consolas, 'Roboto Mono', 'Liberation Mono', monospace;
          font-size: 14px;
          line-height: 1.6;
          min-height: 300px;
        }

        ::ng-deep .ql-editor {
          min-height: 300px;
          max-height: 400px;
          overflow-y: auto;
        }

        ::ng-deep .ql-toolbar {
          border-top-left-radius: 4px;
          border-top-right-radius: 4px;
          background: #f5f5f5;
          border-bottom: 1px solid rgba(0, 0, 0, 0.12);
        }
      }

      .editor-hint {
        font-size: 12px;
        color: rgba(0, 0, 0, 0.6);
      }
    }
  `]
})
export class PromptCreateDialogComponent {
key = '';
name = '';
text = '';

// Quill editor configuration
quillModules = {
  toolbar: [
    ['bold', 'italic', 'underline', 'strike'],
    ['blockquote', 'code-block'],
    [{ 'header': 1 }, { 'header': 2 }],
    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
    [{ 'indent': '-1'}, { 'indent': '+1' }],
    ['link'],
    ['clean']
  ]
};

  constructor(
    private dialogRef: MatDialogRef<PromptCreateDialogComponent>
  ) {}

  onKeyInput(): void {
    // Auto-convert to uppercase and remove invalid characters
    this.key = this.key.toUpperCase().replace(/[^A-Z_]/g, '');
  }

  isValid(): boolean {
    return this.key.trim().length > 0 && 
           this.name.trim().length > 0 &&
           this.text.trim().length >= 10 &&
           /^[A-Z_]+$/.test(this.key);
  }

  onCreate(): void {
    if (this.isValid()) {
      const request: PromptCreateRequest = {
        key: this.key.trim(),
        name: this.name.trim(),
        text: this.text.trim()
      };
      this.dialogRef.close(request);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
