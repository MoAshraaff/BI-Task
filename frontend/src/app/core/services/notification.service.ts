import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  id: number;
  type: NotificationType;
  message: string;
}

let nextId = 1;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly items = signal<Notification[]>([]);
  readonly notifications = this.items.asReadonly();

  success(message: string): void {
    this.push('success', message);
  }

  error(message: string): void {
    this.push('error', message);
  }

  info(message: string): void {
    this.push('info', message);
  }

  dismiss(id: number): void {
    this.items.update((current) => current.filter((n) => n.id !== id));
  }

  private push(type: NotificationType, message: string): void {
    const notification: Notification = { id: nextId++, type, message };
    this.items.update((current) => [...current, notification]);
    setTimeout(() => this.dismiss(notification.id), 5000);
  }
}
