import { Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { AiChatService } from '../../services/ai-chat.service';
import { Bubble } from '../../models/types';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UserPropmt } from '../../models/user-prompt.mode';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ai-chat',
  imports: [
    CommonModule, ReactiveFormsModule, FormsModule
  ],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss'
})
export class AiChatComponent {
  @ViewChild('q') qInput!: ElementRef<HTMLTextAreaElement>;
  @ViewChild('historyEl') historyEl!: ElementRef<HTMLDivElement>;

  private _aiService = inject(AiChatService);
  private _fB = inject(FormBuilder);

  submitFg = this._fB.group({
    searchCtrl: ['']
  })

  get SearchCtrl(): FormControl {
    return this.submitFg.get('searchCtrl') as FormControl;
  }

  query = '';

  onSubmit(): void {
    const q = (this.SearchCtrl.value as string | null)?.trim();
    if (!q || this.loading()) return;
    this.pushUser(q);
    this.SearchCtrl.setValue('');
    this.callApi(q);
  }

  loading = signal<boolean>(false);
  history = signal<Bubble[]>([]);

  isListening = signal<boolean>(false);

  private pushUser(text: string): void {
    console.log(text);
    
    this.history.update(h => [...h, { role: 'user', text }]);
    this.loading.set(true);
    this.scrollToBottom();
  }

  private callApi(prompt: string): void {
    let req = new UserPropmt();
    req.prompt = prompt;

    console.log(req.prompt);
        
    
    this._aiService.recommend(req).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res) {
          this.history.update(h => [...h, { role: 'agent', text: res.message ?? 'Here are some tracks for you 🎧', meta: res }]);
          this.scrollToBottom();
        }
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error.message ?? err.error.message ?? 'Unkown error';
        this.history.update(h => [...h, { role: 'agent', text: `error: ${msg}`, createdAt: new Date() }]);
        this.scrollToBottom();
      }
    })
  }

  private scrollToBottom(): void {
    queueMicrotask(() => {
      if (!this.historyEl) return;
      const el = this.historyEl.nativeElement;
      el.scrollTop = el.scrollHeight;
    })
  }

  onVoiceInput(): void {
    this.isListening.update(v => !v);
  }

  onKeyDown(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      this.onSubmit();
    }
  }

  setSuggestion(text: string): void {
    this.SearchCtrl.setValue(text);
    if (this.qInput) {
      this.qInput.nativeElement.focus();
    }
  }
}
