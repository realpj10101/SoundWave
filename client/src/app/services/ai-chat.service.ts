import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { UserPropmt } from '../models/user-prompt.mode';
import { Observable } from 'rxjs';
import { AiRecommend } from '../models/ai-recommend.model';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class AiChatService {
  private _http = inject(HttpClient);

  private readonly _baseApiUrl = environment.apiUrl + 'api/ai/';

  recommend(request: UserPropmt): Observable<AiRecommend> {
    console.log(request);
    

    return this._http.post<AiRecommend>(this._baseApiUrl + 'recommend', request);
  }
}
