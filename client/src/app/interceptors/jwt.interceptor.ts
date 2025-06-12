import { isPlatformBrowser } from '@angular/common';
import { HttpInterceptorFn } from '@angular/common/http';
import { PLATFORM_ID, inject } from '@angular/core';
import { LoggedInUser } from '../models/account.model';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const plarfromId = inject(PLATFORM_ID);

  if (isPlatformBrowser(plarfromId)) {
    const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

    if (loggedInUserStr) {
      const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${loggedInUser.token}`
        }
      });
    }
  }

  return next(req);
};
