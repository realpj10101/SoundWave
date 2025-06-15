import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { PaginationHandler } from '../extensions/paginationHandler';
import { AudioParams } from '../models/helpers/audio-params';
import { Observable } from 'rxjs';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { Audio } from '../models/audio.model';

@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private _http = inject(HttpClient);

  private readonly _apiUrl = environment.apiUrl + 'api/audiofile';
  private paginationHendler = new PaginationHandler();

  getAll(audioParams: AudioParams): Observable<PaginatedResult<Audio[]>> {
    const params = this.getHttpParams(audioParams);

    return this.paginationHendler.getPaginatedResult<Audio[]>(this._apiUrl, params);
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
