import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { firstValueFrom } from 'rxjs';
import { ParameterService, Parameter } from '../../services/parameter.service';
import { PromptService, Prompt } from '../../services/prompt.service';

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
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule
  ],
  templateUrl: './ai-settings.component.html',
  styleUrls: ['./ai-settings.component.scss']
})
export class AiSettingsComponent implements OnInit {
  parameters: Parameter[] = [];
  prompts: Prompt[] = [];
  loading = false;
  saving = false;

  // AI Provider options
  aiProviders = [
    { value: 'OpenAI', label: 'OpenAI GPT-4o' },
    { value: 'DeepSeek', label: 'DeepSeek' }
  ];

  constructor(
    private parameterService: ParameterService,
    private promptService: PromptService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    
    Promise.all([
      firstValueFrom(this.parameterService.getAll()),
      firstValueFrom(this.promptService.getAll())
    ]).then(([parameters, prompts]) => {
      this.parameters = parameters || [];
      this.prompts = prompts || [];
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
}
