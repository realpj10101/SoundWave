import { Component, ElementRef, inject, Input, OnInit, Output, signal, ViewChild, EventEmitter, PLATFORM_ID, computed } from '@angular/core';
import { Audio } from '../../../models/audio.model';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { LikeService } from '../../../services/like.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { take } from 'rxjs';
import { ApiResponse } from '../../../models/helpers/apiResponse.model';
import { environment } from '../../../../environments/environment.development';
import { PlaylistService } from '../../../services/playlist.service';
import { MatDialog } from '@angular/material/dialog';
import { TargetAudioCardComponent } from '../target-audio-card/target-audio-card.component';
import { AudioPlayerService } from '../../../services/audio-player.service';

@Component({
  selector: 'app-audio-card',
  imports: [
    MatIconModule, CommonModule,
  ],
  templateUrl: './audio-card.component.html',
  styleUrl: './audio-card.component.scss'
})
export class AudioCardComponent implements OnInit {
  @Input() audioInput!: Audio;
  @Input() pageIndex!: number;
  @Input() itemIndex!: number;
  @ViewChild('audioElem') audioElem!: ElementRef<HTMLAudioElement>;
  @Output('dislikeAudioNameOut') dislikeAudioNameOut = new EventEmitter<string>();
  @Output('removeAudioOut') removedAudioOut = new EventEmitter<string>();

  private _likeService = inject(LikeService);
  private _playlistService = inject(PlaylistService);
  private _audioPlayerService = inject(AudioPlayerService);
  private _snack = inject(MatSnackBar);
  private _platformId = inject(PLATFORM_ID);

  readonly dialog = inject(MatDialog);

  count: number | undefined;
  adderCount: number | undefined;
  apiUrl = environment.apiUrl

  isPlaying = computed(() => {
    const currentId = this._audioPlayerService.currentAudioIdSig();

    return currentId === this.audioInput.id &&
      this._audioPlayerService.isPlayingSig();
  });

  bars: number[] = [];

  ngOnInit() {
    this.generateBars();
    this.adderCount = this.audioInput?.addersCount;
  }

  openDialog(): void {
    if (isPlatformBrowser(this._platformId)) {
      const isMobile = window.innerWidth <= 768;

      const dialogRef = this.dialog.open(TargetAudioCardComponent, {
        width: isMobile ? '100vw' : '670px',
        maxWidth: '100vw',
        position: isMobile ? { bottom: '0', left: '0' } : undefined,
        panelClass: isMobile ? 'mobile-audio-card-dialog' : undefined,

        data: {
          audio: this.audioInput,
          pageIndex: this.pageIndex,
          itemIndex: this.itemIndex
        }
      })
    }
  }

  togglePlay() {
    if (this.isPlaying()) {
      this._audioPlayerService.pause();
    } else {
      this._audioPlayerService.loadAndPlay(this.audioInput.id);
    }
  }

  generateBars() {
    this.bars = Array.from({ length: 50 }, () => Math.floor(Math.random() * 100));
  }

  like(): void {
    if (this.audioInput)
      this._likeService.create(this.audioInput.id).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isLiking = true;
              this.audioInput.likersCount++

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
      this._likeService.delete(this.audioInput.id).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isLiking = false;
              this.dislikeAudioNameOut.emit(this.audioInput.fileName);
            }

            this.audioInput.likersCount--;

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
        next: (res: number) => {
          this.count = res

          this.audioInput!.likersCount = res;
        }
      });
  }

  addToPlaylist(): void {
    if (this.audioInput)
      this._playlistService.add(this.audioInput.id).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput)
              this.audioInput.isAdding = true;
            this.audioInput.addersCount++;

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
      this._playlistService.remove(this.audioInput.id).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isAdding = false;
              this.removedAudioOut.emit(this.audioInput.fileName)
            }
            this.audioInput.addersCount--;

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
        next: (res: number) => {
          this.adderCount = res
        }
      })
  }
}
