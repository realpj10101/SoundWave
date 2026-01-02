import { CommonModule } from '@angular/common';
import { Component, ElementRef, EventEmitter, inject, OnInit, Output, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { environment } from '../../../../environments/environment.development';
import { MatIconModule } from '@angular/material/icon';
import { Audio } from '../../../models/audio.model';
import { LikeService } from '../../../services/like.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PlaylistService } from '../../../services/playlist.service';
import { take } from 'rxjs';

@Component({
  selector: 'app-target-audio-card',
  imports: [
    CommonModule, MatIconModule
  ],
  templateUrl: './target-audio-card.component.html',
  styleUrl: './target-audio-card.component.scss'
})
export class TargetAudioCardComponent implements OnInit {
  @ViewChild('audioElement') audioRef!: ElementRef<HTMLAudioElement>;
  @Output('dislikeAudioNameOut') dislikeAudioNameOut = new EventEmitter<string>();

  private _likeService = inject(LikeService);
  private _snack = inject(MatSnackBar);
  private _playlistService = inject(PlaylistService);

  audioData: Audio = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<TargetAudioCardComponent>);

  currentTime = '0:00';
  apiUrl = environment.apiUrl;
  isPlaying = false;
  progressPercentage = 0;
  count: number | undefined;

  ngOnInit(): void {
    this.count = this.audioData.likersCount || 0;
  }

  like(): void {
    if (this.audioData) {
      this._likeService.create(this.audioData.fileName).pipe(take(1))
        .subscribe({
          next: (res) => {
            this.audioData.isLiking = true;
            this.getLikeCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            })
          }
        })
    }
  }

  dislike(): void {
    if (this.audioData) {
      this._likeService.delete(this.audioData.fileName).pipe(take(1))
      .subscribe({
        next: (res) => {
          this.audioData.isLiking = false;
          this.dislikeAudioNameOut.emit(this.audioData.fileName);
          this.getLikeCount();

          this._snack.open(res.message, 'Close', {
            duration: 7000,
            verticalPosition: 'top',
            horizontalPosition: 'center'
          })
        }
      })
    }
  }

  getLikeCount(): void {
    if (this.audioData) {
      this._likeService.getLikesCount(this.audioData.fileName).subscribe({
        next: (res: number) => {
          this.count = res;

          this.audioData.likersCount = res;
        } 
      })
    }
  }

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
