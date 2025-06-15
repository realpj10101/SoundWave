import { Component, ElementRef, Input, signal, ViewChild } from '@angular/core';
import { Audio } from '../../models/audio.model';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-audio-card',
  imports: [
    MatIconModule, CommonModule
  ],
  templateUrl: './audio-card.component.html',
  styleUrl: './audio-card.component.scss'
})
export class AudioCardComponent {
  @Input('audioInput') audioInput: Audio | undefined;
  @ViewChild('audioElem') audioElem!: ElementRef<HTMLAudioElement>;

  isPlaying = signal(false);
  bars: number[] = [];

  ngOnInit() {
    this.generateBars();
  }

  togglePlay() {
    const audio = this.audioElem.nativeElement;
    if (this.isPlaying()) {
      audio.pause();
      this.isPlaying.set(false);
    } else {
      audio.play();
      this.isPlaying.set(true);
    }
  }

  generateBars() {
    this.bars = Array.from({ length: 50 }, () => Math.floor(Math.random() * 100));
  }
}
