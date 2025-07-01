import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private _http = inject(HttpClient);
  private readonly _apiUrl = environment.apiUrl;

  add(targetAudioName: string): Observable<ApiResponse> {
    return this._http.post<ApiResponse>(this._apiUrl + 'add/' + targetAudioName, null);
  }

  remove(targetAudioName: string): Observable<ApiResponse> {
    return this._http.delete<ApiResponse>(this._apiUrl + 'remove/' + targetAudioName);
  }
}
