import { Component, ElementRef, inject, Input, signal, ViewChild } from '@angular/core';
import { Audio } from '../../models/audio.model';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { LikeService } from '../../services/like.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { take } from 'rxjs';
import { ApiResponse } from '../../models/helpers/apiResponse.model';

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
  private _likeService = inject(LikeService);
  private _snack = inject(MatSnackBar);

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

  like(): void {
    if (this.audioInput)
      this._likeService.create(this.audioInput.fileName).pipe(
        take(1))
        .subscribe({
          next: (res: ApiResponse) => {
            if (this.audioInput) {
              this.audioInput.isLiking = true;

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
            if (this.audioInput)
              this.audioInput.isLiking = false;

            this._snack.open(res.message, 'close', {
              duration: 7000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            })
          }
        })
  }
}
