export type UserRole = 'Admin' | 'User';

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  username: string;
  role: UserRole;
}

export interface UserResponse {
  id: number;
  username: string;
  email: string;
  role: UserRole;
}
