import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { AuthResponse, LoginRequest, RegisterRequest, UserResponse, UserRole } from '../models/auth.model';

interface StoredSession {
  token: string;
  username: string;
  role: UserRole;
  expiresAtUtc: string;
}

const STORAGE_KEY = 'bitask.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly username = computed(() => this.session()?.username ?? null);
  readonly role = computed(() => this.session()?.role ?? null);
  readonly isAdmin = computed(() => this.session()?.role === 'Admin');

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  login(request: LoginRequest, remember: boolean = true): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/login`, request).pipe(
      tap((response) => this.storeSession(response, remember))
    );
  }

  register(request: RegisterRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${API_BASE_URL}/api/auth/register`, request);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    sessionStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  private storeSession(response: AuthResponse, remember: boolean): void {
    const stored: StoredSession = {
      token: response.token,
      username: response.username,
      role: response.role,
      expiresAtUtc: response.expiresAtUtc
    };
    localStorage.removeItem(STORAGE_KEY);
    sessionStorage.removeItem(STORAGE_KEY);
    (remember ? localStorage : sessionStorage).setItem(STORAGE_KEY, JSON.stringify(stored));
    this.session.set(stored);
  }

  private readStoredSession(): StoredSession | null {
    const raw = localStorage.getItem(STORAGE_KEY) ?? sessionStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as StoredSession;
      if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        sessionStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return parsed;
    } catch {
      localStorage.removeItem(STORAGE_KEY);
      sessionStorage.removeItem(STORAGE_KEY);
      return null;
    }
  }
}
