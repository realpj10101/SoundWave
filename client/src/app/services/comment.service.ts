import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment.development';
import {CommentResponse, CreateComment} from '../models/comment.model';
import {Observable} from 'rxjs';
import {ApiResponse} from '../models/helpers/apiResponse.model';
@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private _http = inject(HttpClient);

  private  readonly _baseApiUrl = environment.apiUrl  + 'api/comment/';

  create(req: CreateComment, audioId: string): Observable<CommentResponse> {
    return this._http.post<CommentResponse>(this._baseApiUrl + 'add/' + audioId, req);
  }

  getAllAudioComments(audioId: string): Observable<CommentResponse[]> {
    return this._http.get<CommentResponse[]>(this._baseApiUrl + 'get-comments/' + audioId);
  }
 }
