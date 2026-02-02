import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { QuillModule } from 'ngx-quill';
import { Prompt, PromptService } from '../../../services/prompt.service';

export interface FullscreenEditorDialogData {
  prompt: Prompt;
  quillModules: any;
}

@Component({
  selector: 'app-fullscreen-editor-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    QuillModule
  ],
  templateUrl: './fullscreen-editor-dialog.component.html',
  styleUrls: ['./fullscreen-editor-dialog.component.scss']
})
export class FullscreenEditorDialogComponent {
  promptText: string;
  promptName: string;
  quillModules: any;
  saving = false;

  constructor(
    public dialogRef: MatDialogRef<FullscreenEditorDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FullscreenEditorDialogData,
    private promptService: PromptService,
    private snackBar: MatSnackBar
  ) {
    this.promptText = data.prompt.text;
    this.promptName = data.prompt.name;
    this.quillModules = data.quillModules;
  }

  onSave(): void {
    this.saving = true;
    this.promptService.update(this.data.prompt.publicId, { 
      name: this.data.prompt.name, 
      text: this.promptText 
    }).subscribe({
      next: () => {
        this.snackBar.open('Prompt saved successfully', 'Close', { duration: 2000 });
        this.data.prompt.text = this.promptText;
        this.saving = false;
        this.dialogRef.close(this.promptText);
      },
      error: (error) => {
        console.error('Error saving prompt:', error);
        this.snackBar.open('Error saving prompt', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}
