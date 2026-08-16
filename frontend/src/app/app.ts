import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { NavBar } from './layout/nav-bar/nav-bar';
import { ToastHost } from './layout/toast-host/toast-host';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar, ToastHost],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly router = inject(Router);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  protected readonly showNavBar = computed(() => {
    const url = this.currentUrl();
    return !url.startsWith('/login') && !url.startsWith('/register');
  });
}
