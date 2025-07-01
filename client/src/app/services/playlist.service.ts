import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';
import { PlaylistParams } from '../models/helpers/playlist-params';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { Audio } from '../models/audio.model';
import { PaginationHandler } from '../extensions/paginationHandler';

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private _http = inject(HttpClient);
  private _paginationHandler = new PaginationHandler();

  private readonly _apiUrl = environment.apiUrl + 'api/playlist/';

  add(targetAudioName: string): Observable<ApiResponse> {
    return this._http.post<ApiResponse>(this._apiUrl + 'add/' + targetAudioName, null);
  }

  remove(targetAudioName: string): Observable<ApiResponse> {
    return this._http.delete<ApiResponse>(this._apiUrl + 'remove/' + targetAudioName);
  }

  getAddersCount(targetAudioName: string): Observable<number> {
    return this._http.get<number>(this._apiUrl + 'get-adders-count/' + targetAudioName);
  }

  getAll(playlistParams: PlaylistParams): Observable<PaginatedResult<Audio[]>> {
    let params = new HttpParams();

    if (playlistParams) {
      params = params.append('pageNumber', playlistParams.pageNumber);
      params = params.append('pageSize', playlistParams.pageSize);
      params = params.append('predicate', playlistParams.predicate);
    }

    return this._paginationHandler.getPaginatedResult<Audio[]>(this._apiUrl, params); 
  }
}
