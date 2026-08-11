import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notifications = inject(NotificationService);

  if (auth.isAdmin()) {
    return true;
  }

  notifications.error('This page is only available to Admin accounts.');
  return router.createUrlTree(['/products']);
};
