import { Component, ElementRef, HostListener, inject, OnInit, PLATFORM_ID, ViewChild } from '@angular/core';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { AudioService } from '../../services/audio.service';
import { Observable, Subscription, take } from 'rxjs';
import { Audio } from '../../models/audio.model';
import { Pagination } from '../../models/helpers/pagination';
import { AudioParams } from '../../models/helpers/audio-params';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginatedResult } from '../../models/helpers/paginatedResult';
import { AudioCardComponent } from '../audio/audio-card/audio-card.component';
import { isPlatformBrowser } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard',
  imports: [
    SidebarComponent, AudioCardComponent, MatPaginatorModule, FormsModule, ReactiveFormsModule, MatIconModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  @ViewChild('sidebar', { read: ElementRef}) sidebar!: ElementRef<HTMLElement>;
  @ViewChild('sidebarBtn', { read: ElementRef}) sidebarBtn!: ElementRef<HTMLElement>;

  private audioService = inject(AudioService);
  private _platformId = inject(PLATFORM_ID);

  isSidebarOpen = false;
  audios$: Observable<Audio[] | null> | undefined;
  subscribed: Subscription | undefined;
  pagination: Pagination | undefined;
  audios: Audio[] | undefined;
  audioParams: AudioParams | undefined;
  pageSizeOptions = [5, 10, 25];
  pageEvent: PageEvent | undefined;

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSideBar() {
    this.isSidebarOpen = false;
  }

  private _fB = inject(FormBuilder);

  filterFg = this._fB.group({
    searchCtrl: ['', []],
  });

  get SearchCtrl(): FormControl {
    return this.filterFg.get('searchCtrl') as FormControl;
  }

  ngOnInit(): void {
    this.audioParams = new AudioParams();

    if (isPlatformBrowser(this._platformId)) {
      const userStr = localStorage.getItem('loggedInUser');
      if (userStr) {
        const user = JSON.parse(userStr);
        if (user.token) {
          this.getAll();
        }
      }
    }
  }

  getAll(): void {
    if (this.audioParams)
      this.audioService.getAll(this.audioParams).pipe(take(1)).subscribe({
        next: (response: PaginatedResult<Audio[]>) => {
          if (response.body && response.pagination) {
            this.audios = response.body;
            this.pagination = response.pagination;
          }
        }
      })
  }

  handlePageEvent(e: PageEvent) {
    if (this.audioParams) {
      if (e.pageSize !== this.audioParams.pageSize)
        e.pageIndex = 0;

      this.pageEvent = e;
      this.audioParams.pageSize = e.pageSize;
      this.audioParams.pageNumber = e.pageIndex + 1;

      this.getAll();
    }
  }

  updateAudioParams(): void {
    if (this.audioParams) {
      this.audioParams.search = this.SearchCtrl.value;
    }
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    if (isPlatformBrowser(this._platformId)) {
      this.closeSideBar();
    }
  }

  // @HostListener('document:click', ['$event'])
  // onClick(event: MouseEvent): void {
  //   const target = event.target as Node | null;

  //   const btnEl = this.sidebarBtn.nativeElement as HTMLElement | undefined;
  //   const sidebarEl = this.sidebar.nativeElement as HTMLElement | undefined;
    
  //   if (btnEl?.contains(target as Node)) {
  //     return;
  //   }

  //   if (this.isSidebarOpen && sidebarEl && !sidebarEl.contains(target as Node)) {
  //     this.closeSideBar();
  //   }
  // }
}
