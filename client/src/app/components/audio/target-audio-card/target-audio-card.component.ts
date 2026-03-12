import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, computed, ElementRef, EventEmitter, inject, OnInit, Output, PLATFORM_ID, ViewChild } from '@angular/core';
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
import { AudioPlayerService } from '../../../services/audio-player.service';
import { TargetAudioDialogData } from '../../../models/target-audio-dialog.model';
import { AudioService } from '../../../services/audio.service';

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
  private _playlistService = inject(PlaylistService);
  private _audioPlayerService = inject(AudioPlayerService);
  private _audioService = inject(AudioService);
  private _snack = inject(MatSnackBar);
  private _platformId = inject(PLATFORM_ID);
  private _fB = inject(FormBuilder);
  private _commentService = inject(CommentService);

  isPlaying = computed(() => {
    const currentId = this._audioPlayerService.currentAudioIdSig();

    return currentId === this.audioData.id &&
      this._audioPlayerService.isPlayingSig();
  });

  currentTime = computed(() =>
    this.formatTime(this._audioPlayerService.currentTime())
  );

  progressPercentage = computed(() =>
    (this._audioPlayerService.progressSig())
  );

  dialogData = inject<TargetAudioDialogData>(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<TargetAudioCardComponent>);

  audioData: Audio = this.dialogData.audio;
  pageIndex: number = this.dialogData.pageIndex;
  itemIndex: number = this.dialogData.itemIndex;

  apiUrl = environment.apiUrl;
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
        this.comments = [res, ...this.comments];
        this.ContentCtrl.setValue('');
      }
    })
  }

  getAudioComments(): void {
    this._commentService.getAllAudioComments(this.audioData.id).subscribe({
      next: (res: CommentResponse[]) => {
        this.comments = res ?? [];
      },
      error: (err) => {
        this.comments = [];
      }
    })
  }

  like(): void {
    if (this.audioData) {
      this._likeService.create(this.audioData.id).pipe(take(1))
        .subscribe({
          next: (res) => {
            this.audioData.isLiking = true;
            this.audioData.likersCount++;

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
            this.audioData.likersCount--;

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
            this.audioData.addersCount++;

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
            this.audioData.addersCount--;

            this._snack.open(res.message, 'Close', {
              duration: 7000,
              verticalPosition: 'top',
              horizontalPosition: 'center'
            });
          }
        })
    }
  }

  togglePlay() {
    if (this.isPlaying()) {
      this._audioPlayerService.pause();
    } else {
      this._audioPlayerService.loadAndPlay(this.audioData.id);
    }
  }

  goToNextComp(): void {
    this._audioService.goToNextAudio(this.audioData.id).subscribe({
      next: (res) => {
        this.audioData = res;

        const itemsPerPage = 5;

        let newPageIndex = this.pageIndex;
        let newItemIndex = this.itemIndex + 1;

        if (newItemIndex >= itemsPerPage) {
          newPageIndex = this.pageIndex + 1;
          newItemIndex = 0;
        }

        this.pageIndex = newPageIndex;
        this.itemIndex = newItemIndex;

        this._audioPlayerService.loadAndPlay(
          res.id,

        );
      },
      error: (err) => {
        this._snack.open(err.error, 'Close', {
          duration: 5000,
          verticalPosition: 'top',
          horizontalPosition: 'center'
        })
      }
    })
  }

  goToPreviousComp(): void {
    const currentTime = this._audioPlayerService.currentTime();

    if (currentTime >= 5) {
      this._audioPlayerService.seekTo(0);
      this._audioPlayerService.loadAndPlay(
        this.audioData.id
      );
      return;
    }

    this._audioService.goToPrevious(this.audioData.id).subscribe({
      next: (res) => {
        this.audioData = res;

        const itemsPerPage = 5;
        let newPageIndex = this.pageIndex;
        let newItemIndex = this.itemIndex - 1;

        if (newItemIndex < 0) {
          newPageIndex = this.pageIndex - 1;
          newItemIndex = itemsPerPage - 1;
        }

        this.pageIndex = newPageIndex;
        this.itemIndex = newItemIndex;

        this._audioPlayerService.loadAndPlay(
          res.id
        );
      },
      error: (err) => {
        this._snack.open(err.error, 'Close', {
          duration: 5000,
          verticalPosition: 'top',
          horizontalPosition: 'center'
        });
      }
    })
  }

  closedDialog(): void {
    this.dialogRef.close();
  }

  seek(event: MouseEvent): void {
    const progressbar = event.currentTarget as HTMLElement;

    const clickPosition = event.offsetX;
    const totalWidth = progressbar.clientWidth;

    const ratio = clickPosition / totalWidth;

    this._audioPlayerService.seekTo(ratio * 100);
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
