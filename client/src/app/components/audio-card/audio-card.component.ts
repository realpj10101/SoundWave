import { Component, ElementRef, inject, Input, OnInit, Output, signal, ViewChild, EventEmitter } from '@angular/core';
import { Audio } from '../../models/audio.model';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { LikeService } from '../../services/like.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { take } from 'rxjs';
import { ApiResponse } from '../../models/helpers/apiResponse.model';
import { environment } from '../../../environments/environment.development';
import { PlaylistService } from '../../services/playlist.service';

@Component({
  selector: 'app-audio-card',
  imports: [
    MatIconModule, CommonModule
  ],
  templateUrl: './audio-card.component.html',
  styleUrl: './audio-card.component.scss'
})
export class AudioCardComponent implements OnInit {
  @Input('audioInput') audioInput: Audio | undefined;
  @ViewChild('audioElem') audioElem!: ElementRef<HTMLAudioElement>;
  @Output('dislikeAudioNameOut') dislikeAudioNameOut = new EventEmitter<string>();
  @Output('removeAudioOut') removedAudioOut = new EventEmitter<string>();
  private _likeService = inject(LikeService);
  private _snack = inject(MatSnackBar);
  private _playlistService = inject(PlaylistService);
  count: number | undefined;
  adderCount: number | undefined;
  apiUrl = environment.apiUrl

  isPlaying = signal(false);
  bars: number[] = [];

  ngOnInit() {
    this.generateBars();
    this.count = this.audioInput?.likersCount;
    this.adderCount = this.audioInput?.addersCount;
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

  like(): void {
    if (this.audioInput)
      this._likeService.create(this.audioInput.fileName).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isLiking = true;
              this.getLikesCount();

              this._snack.open(res.message, 'Close', {
                duration: 7000,
                horizontalPosition: 'center',
                verticalPosition: 'top'
              })
            }
          }
        })
  }

  dislike(): void {
    if (this.audioInput)
      this._likeService.delete(this.audioInput.fileName).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isLiking = false;
              this.dislikeAudioNameOut.emit(this.audioInput.fileName);
            }

            this.getLikesCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            })
          }
        })
  }

  getLikesCount(): void {
    if (this.audioInput)
      this._likeService.getLikesCount(this.audioInput.fileName).subscribe({
        next: (res: number) => this.count = res
      });
  }

  addToPlaylist(): void {
    if (this.audioInput)
      this._playlistService.add(this.audioInput.fileName).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput)
              this.audioInput.isAdding = true;
            this.getAddersCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            })
          }
        })
  }

  removeFromPlaylist(): void {
    if (this.audioInput)
      this._playlistService.remove(this.audioInput.fileName).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isAdding = false;
              this.removedAudioOut.emit(this.audioInput.fileName)
            }
            this.getAddersCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            })
          }
        })
  }

  getAddersCount(): void {
    if (this.audioInput)
      this._playlistService.getAddersCount(this.audioInput.fileName).subscribe({
        next: (res: number) => this.adderCount = res
      })
  }
}
