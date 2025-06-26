import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { UserUpdate } from '../models/user-update.model';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private _http = inject(HttpClient);
  private _apiUrl = environment.apiUrl + 'api/user/';

  updateUser(userInput: UserUpdate): Observable<ApiResponse> {
    return this._http.put<ApiResponse>(this._apiUrl, userInput);
  }

  setMainPhoto(url_165In: string): Observable<ApiResponse> {
    let queryParams = new HttpParams().set('photoUrlIn', url_165In);

    return this._http.put<ApiResponse>(this._apiUrl + 'set-main-photo', null, { params: queryParams });
  }

  deletePhoto(url_165In: string): Observable<ApiResponse> {
    let queryParams = new HttpParams().set('photoUrlIn', url_165In);

    return this._http.put<ApiResponse>(this._apiUrl + 'delete-photo', null, { params: queryParams });
  }
}
