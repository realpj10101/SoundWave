import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { environment } from '../../../../environments/environment.development';
import { MatIconModule } from '@angular/material/icon';
import { Audio } from '../../../models/audio.model';

@Component({
  selector: 'app-target-audio-card',
  imports: [
    CommonModule, MatIconModule
  ],
  templateUrl: './target-audio-card.component.html',
  styleUrl: './target-audio-card.component.scss'
})
export class TargetAudioCardComponent {
  @ViewChild('audioElement') audioRef!: ElementRef<HTMLAudioElement>;

  audioData: Audio = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<TargetAudioCardComponent>);

  currentTime = '0:00';
  apiUrl = environment.apiUrl;
  isPlaying = false;
  progressPercentage = 0;

  togglePlay(): void {
    const audio = this.audioRef.nativeElement;

    if (audio.paused) {
      audio.play();
      this.isPlaying = true;
    }
    else {
      audio.pause();
      this.isPlaying = false;
    }
  }

  closedDialog(): void {
    this.dialogRef.close();
  }

  seek(event: MouseEvent): void {
    const audio = this.audioRef.nativeElement;
    const progressbar = event.currentTarget as HTMLElement;

    const clickPosition = event.offsetX;
    const totalWidth = progressbar.clientWidth;

    const ratio = clickPosition / totalWidth;

    this.progressPercentage = ratio * 100;

    if (audio.duration) {
      const newTime = ratio * audio.duration;

      audio.currentTime = newTime;
    }
  }

  updateProgress(): void {
    const audio = this.audioRef.nativeElement;
    const duration = audio.duration;
    const currentTime = audio.currentTime;

    if (duration > 0) {
      this.progressPercentage = (currentTime / duration) * 100;

      this.currentTime = this.formatTime(currentTime);
    }
  }

  onAudioEnded(): void {
    this.isPlaying = false;
    this.progressPercentage = 0;
    this.currentTime = '0:00';
    this.audioRef.nativeElement.currentTime = 0;
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const partSeconds = Math.floor(seconds % 60);

    const mm = minutes < 10 ? `0${minutes}` : `${minutes}`;
    const ss = partSeconds < 10 ? `0${partSeconds}` : `${partSeconds}`;

    return `${mm}:${ss}`;
  }
}
