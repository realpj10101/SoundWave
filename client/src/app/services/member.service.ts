import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Observable } from 'rxjs';
import { Member } from '../models/member.model';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private _http = inject(HttpClient);
  private _apiUrl = environment.apiUrl + 'api/member/';

  getByUserName(userName: string): Observable<Member | undefined> {
    return this._http.get<Member>(this._apiUrl + 'get-by-username/' + userName);
  }
}
