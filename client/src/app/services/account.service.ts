import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LoggedInUser, Login, Register } from '../models/account.model';
import { map, Observable, take } from 'rxjs';
import { ApiResponse } from '../models/helpers/apiResponse.model';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  http = inject(HttpClient);
  router = inject(Router);
  platformId = inject(PLATFORM_ID);
  loggedInUserSig = signal<LoggedInUser | null>(null);

  private readonly baseApiUrl = environment.apiUrl + 'api/account/';

  registerPlayer(userInput: Register): Observable<LoggedInUser | null> {
    return this.http.post<LoggedInUser>(this.baseApiUrl + 'register', userInput).pipe(
      map(res => {
        if (res) {
          this.setCurrentPlayer(res);

          this.navigateToReturnUrl(); // navigate to url wich player tried before log-in. else: default

          return res;
        }

        return null;
      })
    );
  }

  loginPlayer(userInput: Login): Observable<LoggedInUser | null> {
    return this.http.post<LoggedInUser>(this.baseApiUrl + 'login', userInput).pipe(
      map(res => {
        if (res) {
          this.setCurrentPlayer(res);

          this.navigateToReturnUrl();

          return res;
        }

        return null;
      })
    );
  }

  authorizeLoggedInPlayer(): void {
    this.http.get<ApiResponse>(this.baseApiUrl)
      .pipe(
        take(1))
      .subscribe({
        next: res => {
          if (res.message)
            console.log(res.message);
        },
        error: err => {
          console.log(err.error);
          this.logout()
        }
      });
  }

  setCurrentPlayer(loggedInUser: LoggedInUser): void {
    this.setLoggedInPlayerRoles(loggedInUser);

    this.loggedInUserSig.set(loggedInUser);

    if (isPlatformBrowser(this.platformId)) // we make sure this code is ran on the browser and NOT server
      localStorage.setItem('loggedInUser', JSON.stringify(loggedInUser));
  }

  setLoggedInPlayerRoles(loggedInUser: LoggedInUser): void {
    loggedInUser.roles = []; // We have to initialize it before pushing. Otherwise, it's 'undefined' and push will not work.

    const roles: string | string[] = JSON.parse(atob(loggedInUser.token.split('.')[1])).role; // get the token's 2nd part then select role

    Array.isArray(roles) ? loggedInUser.roles = roles : loggedInUser.roles.push(roles);
  }

  logout(): void {
    this.loggedInUserSig.set(null);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear(); // delete all browser's localStorage's items at once  
    }

    this.router.navigateByUrl('account/login');
  }

  private navigateToReturnUrl(): void {
    if (isPlatformBrowser(this.platformId)) {
      const returnUrl = localStorage.getItem('returnUrl');
      if (returnUrl)
        this.router.navigate([returnUrl]);
      else
        this.router.navigate(['dashboard']);

      if (isPlatformBrowser(this.platformId)) // we make sure this code is ran on the browser and NOT server
        localStorage.removeItem('returnUrl');
    }
  }
}
