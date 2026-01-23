import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, ElementRef, EventEmitter, inject, OnInit, Output, PLATFORM_ID, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { environment } from '../../../../environments/environment.development';
import { MatIconModule } from '@angular/material/icon';
import { Audio } from '../../../models/audio.model';
import { LikeService } from '../../../services/like.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PlaylistService } from '../../../services/playlist.service';
import { take } from 'rxjs';
import ColorThief from 'colorthief';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommentService } from '../../../services/comment.service';
import { CommentResponse, CreateComment } from '../../../models/comment.model';
import { ApiResponse } from '../../../models/helpers/apiResponse.model';
import { IntlModule } from 'angular-ecmascript-intl';
import { Member } from '../../../models/member.model';
import { LoggedInUser } from '../../../models/account.model';

@Component({
  selector: 'app-target-audio-card',
  imports: [
    CommonModule, MatIconModule, ReactiveFormsModule, FormsModule, IntlModule
  ],
  templateUrl: './target-audio-card.component.html',
  styleUrl: './target-audio-card.component.scss'
})
export class TargetAudioCardComponent implements OnInit {
  @ViewChild('audioElement') audioRef!: ElementRef<HTMLAudioElement>;
  @ViewChild('comContainer') comContainer!: ElementRef<HTMLDivElement>;
  @Output('dislikeAudioNameOut') dislikeAudioNameOut = new EventEmitter<string>();

  private _likeService = inject(LikeService);
  private _snack = inject(MatSnackBar);
  private _playlistService = inject(PlaylistService);
  private _platformId = inject(PLATFORM_ID);
  private _fB = inject(FormBuilder);
  private _commentService = inject(CommentService);

  audioData: Audio = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<TargetAudioCardComponent>);

  currentTime = '0:00';
  apiUrl = environment.apiUrl;
  isPlaying = false;
  progressPercentage = 0;
  dynamicGradient = 'linear-gradient(135deg, #111827 0%, #1f2937 50%, #000000 100%)';
  isCommentSectionOpen = false;
  comments: CommentResponse[] = [];

  commentFg = this._fB.group({
    contentCtrl: ['']
  })

  get ContentCtrl(): FormControl {
    return this.commentFg.get('contentCtrl') as FormControl;
  }

  ngOnInit(): void {
    this.getAudioComments();
  }

  scrollToTop(): void {
    setTimeout(() => {
      if (this.comContainer) {
        this.comContainer.nativeElement.scrollTop = 0;
      }
    }, 0);
  }

  toggleCommentSection(): void {
    this.isCommentSectionOpen = !this.isCommentSectionOpen;
  }

  addComment(): void {
    let req: CreateComment = {
      content: this.ContentCtrl.value
    }

    this._commentService.create(req, this.audioData.id).subscribe({
      next: (res) => {
        this.comments = [...this.comments, res];
        this.ContentCtrl.setValue('');
      }
    })
  }

  getAudioComments(): void {
    this._commentService.getAllAudioComments(this.audioData.id).subscribe({
      next: (res: CommentResponse[]) => {
        this.comments = res.reverse();
      }
    })
  }

  like(): void {
    if (this.audioData) {
      this._likeService.create(this.audioData.id).pipe(take(1))
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
      this._likeService.delete(this.audioData.id).pipe(take(1))
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

  addToPlaylist(): void {
    if (this.audioData) {
      this._playlistService.add(this.audioData.id).pipe(take(1))
        .subscribe({
          next: (res) => {
            this.audioData.isAdding = true;
            this.getAddersCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              verticalPosition: 'top',
              horizontalPosition: 'center'
            });
          }
        })
    }
  }

  removeFromPlaylist(): void {
    if (this.audioData) {
      this._playlistService.remove(this.audioData.id).pipe(take(1))
        .subscribe({
          next: (res) => {
            this.audioData.isAdding = false;
            this.getAddersCount();

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              verticalPosition: 'top',
              horizontalPosition: 'center'
            });
          }
        })
    }
  }

  getAddersCount(): void {
    if (this.audioData) {
      this._playlistService.getAddersCount(this.audioData.fileName).subscribe({
        next: (res) => {
          this.audioData.addersCount = res;
        }
      })
    }
  }

  getLikeCount(): void {
    if (this.audioData) {
      this._likeService.getLikesCount(this.audioData.fileName).subscribe({
        next: (res: number) => {
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

  onKeyDown(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      this.addComment();
    }
  }

  async shareAudio() {
    if (isPlatformBrowser(this._platformId)) {
      const shareData = {
        title: this.audioData.fileName,
        text: `Check out "${this.audioData.fileName}" by ${this.audioData.uploaderName}`,
        url: window.location.href
      };

      try {
        if (navigator.share) {
          await navigator.share(shareData);

          this._snack.open('Shared successfully', 'Close', {
            duration: 7000,
            verticalPosition: 'top',
            horizontalPosition: 'center'
          })
        }
        else {
          this.copyToClipboard(shareData.url);
        }
      }
      catch (err) {
        this._snack.open('Error sharing', 'Close', {
          duration: 7000,
          verticalPosition: 'top',
          horizontalPosition: 'center'
        })
      }
    }
  }

  private copyToClipboard(url: string): void {
    navigator.clipboard.writeText(url).then(() => {
      this._snack.open('Link copied to clipboard!', 'Close', {
        duration: 7000,
        verticalPosition: 'top',
        horizontalPosition: 'center'
      })
    })
  }

  extractColors(event: any): void {
    if (isPlatformBrowser(this._platformId)) {
      const img = event.target as HTMLImageElement;

      if (img.complete) {
        this.applyPallete(img);
      }
      else {
        img.onload = () => this.applyPallete(img);
      }
    }
  }

  private applyPallete(img: HTMLImageElement): void {
    try {
      const colorThief = new ColorThief();

      const palette = colorThief.getPalette(img, 3);

      if (palette && palette.length >= 3) {
        const c1 = `rgb(${palette[0].join(',')})`;
        const c2 = `rgb(${palette[1].join(',')})`;
        const c3 = `rgb(${palette[2].join(',')})`;

        this.dynamicGradient = `linear-gradient(135deg, ${c1} 0%, ${c2} 50%, ${c3} 100%)`;
      }
    }
    catch (error) {
      console.error('CORS Error: Color cannot be extract', error);
    }
  }
}
