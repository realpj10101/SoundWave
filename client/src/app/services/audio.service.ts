import { HttpClient, HttpEvent, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { PaginationHandler } from '../extensions/paginationHandler';
import { AudioParams } from '../models/helpers/audio-params';
import { Observable, retry } from 'rxjs';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { Audio } from '../models/audio.model';

@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private _http = inject(HttpClient);

  private readonly _apiUrl = environment.apiUrl + 'api/audiofile/';
  private paginationHendler = new PaginationHandler();

  getAll(audioParams: AudioParams): Observable<PaginatedResult<Audio[]>> {
    const params = this.getHttpParams(audioParams);

    return this.paginationHendler.getPaginatedResult<Audio[]>(this._apiUrl, params);
  }

  getUserAudios(audioParams: AudioParams): Observable<PaginatedResult<Audio[]>> {
    const params = this.getHttpParams(audioParams);
    
    return this.paginationHendler.getPaginatedResult<Audio[]>(this._apiUrl + 'get-user-audios', params);
  }

  stream(id: string): Observable<HttpEvent<Blob>> {
    return this._http.get<Blob>(this._apiUrl + 'stream/' + id, {
      responseType: 'blob' as 'json',
      observe: 'events',
      reportProgress: true
    })
  }

  goToNextAudio(id: string): Observable<Audio> {
    return this._http.get<Audio>(this._apiUrl + 'next-audio/' + id);
  }

  goToPrevious(id: string): Observable<Audio> {
    return this._http.get<Audio>(this._apiUrl + 'previous-audio/' + id);
  }

  private getHttpParams(audioParams: AudioParams): HttpParams {
    let params = new HttpParams();

    if (audioParams) {
      params = params.append('search', audioParams.search);
      params = params.append('pageSize', audioParams.pageSize);
      params = params.append('pageNumber', audioParams.pageNumber);
    }

    return params;
  } 
}
