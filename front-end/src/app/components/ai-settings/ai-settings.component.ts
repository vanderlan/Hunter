import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { firstValueFrom } from 'rxjs';
import { ParameterService, Parameter, ParameterCreateRequest } from '../../services/parameter.service';
import { PromptService, Prompt, PromptCreateRequest } from '../../services/prompt.service';
import { ParameterCreateDialogComponent } from './parameter-create-dialog/parameter-create-dialog.component';
import { PromptCreateDialogComponent } from './prompt-create-dialog/prompt-create-dialog.component';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { QuillModule } from 'ngx-quill';

@Component({
  selector: 'app-ai-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatDialogModule,
    MatTooltipModule,
    QuillModule
  ],
  templateUrl: './ai-settings.component.html',
  styleUrls: ['./ai-settings.component.scss']
})
export class AiSettingsComponent implements OnInit {
  parameters: Parameter[] = [];
  prompts: Prompt[] = [];
  loading = false;
  saving = false;

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

  // AI Provider options
  aiProviders = [
    { 
      value: 'OpenAI', 
      label: 'OpenAI GPT-4o',
      description: 'Most accurate AI model with advanced reasoning capabilities'
    },
    { 
      value: 'DeepSeek', 
      label: 'DeepSeek',
      description: 'Cost-effective alternative with comparable performance'
    }
  ];

  activeAIProvider = 'OpenAI';

  constructor(
    private parameterService: ParameterService,
    private promptService: PromptService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  get regularParameters(): Parameter[] {
    return this.parameters.filter(p => p.key !== 'ACTIVE_AI_SERVICE');
  }

  onProviderChange(): void {
    const aiServiceParam = this.parameters.find(p => p.key === 'ACTIVE_AI_SERVICE');
    if (aiServiceParam) {
      aiServiceParam.value = this.activeAIProvider;
      this.saveParameter(aiServiceParam);
    } else {
      // Create the parameter if it doesn't exist
      this.createParameter({
        key: 'ACTIVE_AI_SERVICE',
        value: this.activeAIProvider
      });
    }
  }

  loadData(): void {
    this.loading = true;
    
    Promise.all([
      firstValueFrom(this.parameterService.getAll()),
      firstValueFrom(this.promptService.getAll())
    ]).then(([parameters, prompts]) => {
      this.parameters = parameters || [];
      this.prompts = prompts || [];
      
      // Set active AI provider
      const aiServiceParam = this.parameters.find(p => p.key === 'ACTIVE_AI_SERVICE');
      if (aiServiceParam) {
        this.activeAIProvider = aiServiceParam.value;
      }
      
      this.loading = false;
    }).catch(error => {
      console.error('Error loading AI settings:', error);
      this.snackBar.open('Error loading AI settings', 'Close', { duration: 3000 });
      this.loading = false;
    });
  }

  getParameterByKey(key: string): Parameter | undefined {
    return this.parameters.find(p => p.key === key);
  }

  getParameterValue(key: string): string {
    const param = this.getParameterByKey(key);
    return param?.value || '';
  }

  setParameterValue(key: string, value: string): void {
    const param = this.getParameterByKey(key);
    if (param) {
      param.value = value;
    }
  }

  getPromptByKey(key: string): Prompt | undefined {
    return this.prompts.find(p => p.key === key);
  }

  openCreateParameterDialog(): void {
    const dialogRef = this.dialog.open(ParameterCreateDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe((request: ParameterCreateRequest | null) => {
      if (request) {
        this.createParameter(request);
      }
    });
  }

  openCreatePromptDialog(): void {
    const dialogRef = this.dialog.open(PromptCreateDialogComponent, {
      width: '750px',
      maxHeight: '90vh'
    });

    dialogRef.afterClosed().subscribe((request: PromptCreateRequest | null) => {
      if (request) {
        this.createPrompt(request);
      }
    });
  }

  createParameter(request: ParameterCreateRequest): void {
    this.saving = true;
    this.parameterService.create(request)
      .subscribe({
        next: (newParameter) => {
          this.parameters.push(newParameter);
          this.snackBar.open('Parameter created successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error creating parameter:', error);
          const errorMsg = error.error?.title || error.error?.message || 'Error creating parameter';
          this.snackBar.open(errorMsg, 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }

  createPrompt(request: PromptCreateRequest): void {
    this.saving = true;
    this.promptService.create(request)
      .subscribe({
        next: (newPrompt) => {
          this.prompts.push(newPrompt);
          this.snackBar.open('Prompt created successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error creating prompt:', error);
          const errorMsg = error.error?.title || error.error?.message || 'Error creating prompt';
          this.snackBar.open(errorMsg, 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }

  saveParameter(param: Parameter): void {
    this.saving = true;
    this.parameterService.update(param.publicId, { value: param.value })
      .subscribe({
        next: () => {
          this.snackBar.open('Parameter saved successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error saving parameter:', error);
          this.snackBar.open('Error saving parameter', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }

  savePrompt(prompt: Prompt): void {
    this.saving = true;
    this.promptService.update(prompt.publicId, { name: prompt.name, text: prompt.text })
      .subscribe({
        next: () => {
          this.snackBar.open('Prompt saved successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error saving prompt:', error);
          this.snackBar.open('Error saving prompt', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }

  confirmDeleteParameter(param: Parameter): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Parameter',
        message: `Are you sure you want to delete the parameter "${param.key}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.deleteParameter(param);
      }
    });
  }

  deleteParameter(param: Parameter): void {
    this.saving = true;
    this.parameterService.delete(param.publicId)
      .subscribe({
        next: () => {
          this.parameters = this.parameters.filter(p => p.publicId !== param.publicId);
          this.snackBar.open('Parameter deleted successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error deleting parameter:', error);
          this.snackBar.open('Error deleting parameter', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }

  openFullscreenEditor(prompt: Prompt): void {
    // Lazy load the fullscreen editor dialog
    import('./fullscreen-editor-dialog/fullscreen-editor-dialog.component').then(module => {
      const dialogRef = this.dialog.open(module.FullscreenEditorDialogComponent, {
        width: '95vw',
        height: '95vh',
        maxWidth: '95vw',
        maxHeight: '95vh',
        panelClass: 'fullscreen-dialog',
        data: {
          prompt: prompt,
          quillModules: this.quillModules
        }
      });

      dialogRef.afterClosed().subscribe((updatedText: string | null) => {
        if (updatedText !== null && updatedText !== undefined) {
          prompt.text = updatedText;
        }
      });
    }).catch(error => {
      console.error('Error loading fullscreen editor:', error);
      this.snackBar.open('Error loading fullscreen editor', 'Close', { duration: 3000 });
    });
  }

  confirmDeletePrompt(prompt: Prompt): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Prompt',
        message: `Are you sure you want to delete the prompt "${prompt.name}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.deletePrompt(prompt);
      }
    });
  }

  deletePrompt(prompt: Prompt): void {
    this.saving = true;
    this.promptService.delete(prompt.publicId)
      .subscribe({
        next: () => {
          this.prompts = this.prompts.filter(p => p.publicId !== prompt.publicId);
          this.snackBar.open('Prompt deleted successfully', 'Close', { duration: 2000 });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error deleting prompt:', error);
          this.snackBar.open('Error deleting prompt', 'Close', { duration: 3000 });
          this.saving = false;
        }
      });
  }
}

