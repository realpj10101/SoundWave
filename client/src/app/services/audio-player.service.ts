import { inject, Injectable, signal } from '@angular/core';
import { AudioService } from './audio.service';
import { HttpEventType } from '@angular/common/http';
import { PlayingTrack } from '../models/plaing-track.model';

@Injectable({
  providedIn: 'root'
})
export class AudioPlayerService {
  private _audioService = inject(AudioService);

  private _audio = new Audio();
  private _currentObjectUrl: string | null = null;

  isPlayingSig = signal<boolean>(false);
  progressSig = signal<number>(0);
  durationSig = signal<number>(0);
  currentTime = signal<number>(0);

  currentAudioIdSig = signal<string | null>(null);

  constructor() {
    this._audio.addEventListener('play', () => {
      this.isPlayingSig.set(true);
    });

    this._audio.addEventListener('pause', () => {
      this.isPlayingSig.set(false);
    });

    this._audio.addEventListener('ended', () => {
      this.isPlayingSig.set(false);
      this.progressSig.set(0);
      this.currentTime.set(0);
    });

    this._audio.addEventListener('timeupdate', () => {
      if (this._audio.duration) {
        const percent =
          (this._audio.currentTime / this._audio.duration) * 100;

        this.progressSig.set(percent);
        this.currentTime.set(this._audio.currentTime);
        this.durationSig.set(this._audio.duration);
      }
    });
  }

  loadAndPlay(audioId: string): void {
    if (this.currentAudioIdSig() === audioId) {
      this._audio.play();
      return;
    }

    this.currentAudioIdSig.set(audioId);

    this._audioService.stream(audioId).subscribe(event => {
      if (event.type === HttpEventType.Response) {
        const blob = event.body as Blob;

        if (this._currentObjectUrl)
          URL.revokeObjectURL(this._currentObjectUrl);

        this._currentObjectUrl = URL.createObjectURL(blob);
        this._audio.src = this._currentObjectUrl;

        this._audio.load();
        this._audio.play();
      }
    });
  }

  pause(): void {
    this._audio.pause();
  }

  seekTo(percent: number): void {
    if (this._audio.duration) {
      this._audio.currentTime = (percent / 100) * this._audio.duration;
    }
  }
}
