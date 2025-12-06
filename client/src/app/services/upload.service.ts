import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class UploadService {
  private _http = inject(HttpClient);

  private readonly _baseApiUrl = environment.apiUrl + 'api/audiofile/';

  upload(fd: FormData): Observable<ApiResponse> {
    return this._http.post<ApiResponse>(this._baseApiUrl + 'upload', fd);
  }
}
