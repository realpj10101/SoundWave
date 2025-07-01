import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';
import { LikeParams } from '../models/helpers/like-params.model';
import { PaginatedResult } from '../models/helpers/paginatedResult';
import { Audio } from '../models/audio.model';
import { PaginationHandler } from '../extensions/paginationHandler';

@Injectable({
  providedIn: 'root'    
})
export class LikeService {
  private _http = inject(HttpClient);
  private _paginationHandler = new PaginationHandler();

  private readonly _apiUrl = environment.apiUrl + 'api/like/';
  
  create(targetAudioName: string): Observable<ApiResponse> {
    return this._http.post<ApiResponse>(this._apiUrl + 'add/' + targetAudioName, null);
  }

  delete(targetAudioName: string): Observable<ApiResponse> {
    return this._http.delete<ApiResponse>(this._apiUrl + 'remove/' + targetAudioName);
  }

  getLikesCount(targetAudioName: string): Observable<number> {
    return this._http.get<number>(this._apiUrl + 'get-likes-count/' + targetAudioName);
  }

  getAll(likeParams: LikeParams): Observable<PaginatedResult<Audio[]>> {
    let params = new HttpParams();

    if (likeParams) {
      params = params.append('pageNumber', likeParams.pageNumber);
      params = params.append('pageSize', likeParams.pageSize);
      params = params.append('predicate', likeParams.predicate);
    }

    return this._paginationHandler.getPaginatedResult<Audio[]>(this._apiUrl, params);
  }
}
