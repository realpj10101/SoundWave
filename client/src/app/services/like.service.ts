import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class LikeService {
  private _http = inject(HttpClient);

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
}
