import { isPlatformBrowser } from '@angular/common';
import { inject, PLATFORM_ID } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CanActivateFn, Router } from '@angular/router';

export const authLoggedInGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const snackbar = inject(MatSnackBar);
  const platformId = inject(PLATFORM_ID);

  if (isPlatformBrowser(platformId)) {
    if (localStorage.getItem('loggedInUser')) {
      router.navigate(['dashboard']);

      snackbar.open('You are already logged in', 'Cloase', {
        horizontalPosition: 'center',
        verticalPosition: 'top',
        duration: 7000
      })

      return false;
    }
  }

  return true;
};
