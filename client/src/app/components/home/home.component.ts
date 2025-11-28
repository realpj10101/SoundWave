import { isPlatformBrowser } from '@angular/common';
import { AfterContentInit, AfterViewInit, Component, ElementRef, HostListener, inject, PLATFORM_ID, ViewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [
    RouterModule, MatIconModule, MatButtonModule
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  @ViewChild('chatBox') chatBox!: ElementRef<HTMLElement>;
  @ViewChild('chatButton') chatButton!: ElementRef<HTMLButtonElement>;

  private _platformId = inject(PLATFORM_ID);

  isChatOpen: boolean = false;

  openChat(event: MouseEvent): void {
    event.stopPropagation();
    this.isChatOpen = true;
  }

  closeChat(): void {
    this.isChatOpen = false;
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (isPlatformBrowser(this._platformId)) {
      this.closeChat();
    }
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as Node | null;

    if (this.chatButton && this.chatButton.nativeElement.contains(target as Node)) {
      return;
    }

    if (this.isChatOpen && this.chatBox && !this.chatBox.nativeElement.contains(target)) {
      this.closeChat();
    }
  }
}
