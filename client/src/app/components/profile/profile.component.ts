import { Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { MatButtonModule } from '@angular/material/button';
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { LikeService } from '../../services/like.service';
import { Audio } from '../../models/audio.model';
import { LikeParams } from '../../models/helpers/like-params.model';
import { PageEvent } from '@angular/material/paginator';
import { PaginatedResult } from '../../models/helpers/paginatedResult';
import { Pagination } from '../../models/helpers/pagination';
import { LikePredicate } from '../../enums/like-predicate-enum';
import { AudioCardComponent } from "../audio-card/audio-card.component";
import { RouterModule } from '@angular/router';
import { AudioService } from '../../services/audio.service';
import { AudioParams } from '../../models/helpers/audio-params';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-profile',
  imports: [
    SidebarComponent, MatButtonModule, MatTabsModule,
    AudioCardComponent, RouterModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  isSidebarOpen = false;
  private _likeService = inject(LikeService);
  private _audioService = inject(AudioService);
  audioParams: AudioParams | undefined;
  audios: Audio[] | undefined;
  readonly likings = 'Liked';
  readonly myTracks = 'My Tracks';
  likeParams = new LikeParams();
  subscribed: Subscription | undefined;
  pagination: Pagination | undefined;

  pageOptions = [3, 9, 12];
  pageEvent: PageEvent | undefined;

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  ngOnInit(): void {
    // this.getAll();  
    
    this.audioParams = new AudioParams();

    this.getUserAudios();
  }

  getAll(): void {
    this.audios = [];

    this._likeService.getAll(this.likeParams).subscribe({
      next: (res: PaginatedResult<Audio[]>) => {
        if (res.body && res.pagination) {
          this.audios = res.body;
          this.pagination = res.pagination;
        }
      },
      error: err => console.log(err)
    })
  }

  getUserAudios(): void {
    if (this.audioParams)
      this.subscribed = this._audioService.getUserAudios(this.audioParams).subscribe({
        next: (response: PaginatedResult<Audio[]>) => {
          if (response.body && response.pagination) {
            this.audios = response.body;
            this.pagination = response.pagination;
          }
        }
      })
  }

  onTabChange(event: MatTabChangeEvent) {
    if (event.tab.textLabel == this.likings) {
      this.likeParams.predicate = LikePredicate.LIKINGS;

      this.getAll();
    }
    else if (event.tab.textLabel == this.myTracks) {
      this.getUserAudios();
    }
  }

  removeDislikeAudioFromAudios(audioName: string): void {
    console.log(audioName);

    const audios = this.audios?.filter(audio => audio.fileName !== audioName);
    this.audios = audios;
  }

  handlePageEvent(e: PageEvent): void {
    if (e.pageSize !== this.likeParams.pageSize)
      e.pageIndex = 0;

    this.pageEvent = e;
    this.likeParams.pageSize = e.pageSize;
    this.likeParams.pageNumber = e.pageIndex + 1;

    this.getAll();
  }
}
