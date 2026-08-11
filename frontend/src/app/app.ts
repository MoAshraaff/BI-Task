import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from './layout/nav-bar/nav-bar';
import { ToastHost } from './layout/toast-host/toast-host';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar, ToastHost],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
