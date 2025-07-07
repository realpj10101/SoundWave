import { isPlatformBrowser } from '@angular/common';
import { inject, PLATFORM_ID } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);
  const platfromId = inject(PLATFORM_ID);

  if (isPlatformBrowser(platfromId)) {
    const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');
    
    if (loggedInUserStr) {
      return true;
    }

    snackBar.open('Please login first.', 'Close', {
      verticalPosition: 'top',
      horizontalPosition: 'center',
      duration: 7000
    });

    localStorage.setItem('returnUrl', state.url);

    router.navigate(['account/login'], { queryParams: { 'returnUrl': state.url } });
  }

  return false;
};
