import { Component, inject, OnInit, PLATFORM_ID, Signal, ViewEncapsulation } from '@angular/core';
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
import { Subscription, take } from 'rxjs';
import { MemberService } from '../../services/member.service';
import { Member } from '../../models/member.model';
import { LoggedInUser } from '../../models/account.model';
import { isPlatformBrowser } from '@angular/common';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment.development';
import { PlaylistParams } from '../../models/helpers/playlist-params';
import { PlaylistService } from '../../services/playlist.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-profile',
  imports: [
    SidebarComponent, MatButtonModule, MatTabsModule,
    AudioCardComponent, RouterModule, MatIconModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  isSidebarOpen = false;
  private _likeService = inject(LikeService);
  private _accountService = inject(AccountService);
  private _audioService = inject(AudioService);
  private _playlistService = inject(PlaylistService);
  private _platformId = inject(PLATFORM_ID);

  readonly likings = 'Liked';
  readonly myTracks = 'My Tracks';
  readonly playlist = 'Playlist'

  audioParams: AudioParams | undefined;
  audios: Audio[] | undefined;
  likeParams = new LikeParams();
  playlistParams = new PlaylistParams();
  subscribed: Subscription | undefined;
  pagination: Pagination | undefined;
  member: Member | undefined;
  loggedInUser: LoggedInUser | undefined;
  loggedInUserSig: Signal<LoggedInUser | null> | undefined;
  apiUrl: string = environment.apiUrl;

  pageOptions = [3, 9, 12];
  pageEvent: PageEvent | undefined;

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  ngOnInit(): void {
    this.loggedInUserSig = this._accountService.loggedInUserSig;

    this.audioParams = new AudioParams();

    if (isPlatformBrowser(this._platformId)) {
      const userStr = localStorage.getItem('loggedInUser');
      if (userStr) {
        const user = JSON.parse(userStr);
        if (user.token) {
          this.getUserAudios();
        }
      }
    }
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

  getUserPlaylist(): void {
    this.audios = [];

    this._playlistService.getAll(this.playlistParams).subscribe({
      next: (res: PaginatedResult<Audio[]>) => {
        if (res.body && res.pagination) {
          this.audios = res.body;
          this.pagination = res.pagination;
        }
      }
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
    else if (event.tab.textLabel == this.playlist) {
      this.getUserPlaylist();
    }
  }

  removeDislikeAudioFromAudios(audioName: string): void {
    console.log(audioName);

    const audios = this.audios?.filter(audio => audio.fileName !== audioName);
    this.audios = audios;
  }

  removeAudioFromPlaylist(audioName: string): void {
    const audios = this.audios?.filter(audio => audio.fileName !== audioName)
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
