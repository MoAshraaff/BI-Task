import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
    rememberMe: [true]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, password, rememberMe } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth.login({ username, password }, rememberMe).subscribe({
      next: (response) => {
        this.notifications.success(`Welcome back, ${response.username}.`);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/products';
        this.router.navigateByUrl(returnUrl);
      },
      error: () => this.submitting.set(false)
    });
  }

  protected socialComingSoon(provider: string): void {
    this.notifications.info(`${provider} sign-in isn't wired up in this demo yet.`);
  }

  protected forgotPassword(): void {
    this.notifications.info('Password reset is not available in this demo yet.');
  }
}
