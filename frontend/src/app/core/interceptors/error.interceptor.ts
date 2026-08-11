import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/** Surfaces every failed API call as a toast, and forces a re-login on an expired/invalid token. */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          auth.logout();
          notifications.error('Your session has expired. Please sign in again.');
          router.navigate(['/login']);
        } else if (error.status === 403) {
          notifications.error("You don't have permission to do that. This action requires an Admin account.");
        } else {
          notifications.error(extractMessage(error));
        }
      } else {
        notifications.error('Something went wrong. Please try again.');
      }

      return throwError(() => error);
    })
  );
};

function extractMessage(error: HttpErrorResponse): string {
  const body = error.error;

  if (typeof body === 'string' && body.trim().length > 0) {
    return body;
  }

  if (body && typeof body === 'object') {
    if (typeof body.message === 'string') {
      return body.message;
    }
    if (typeof body.title === 'string') {
      return body.title;
    }
    if (body.errors && typeof body.errors === 'object') {
      const firstField = Object.values(body.errors)[0];
      if (Array.isArray(firstField) && firstField.length > 0) {
        return firstField[0];
      }
    }
  }

  return `Request failed (${error.status}).`;
}
