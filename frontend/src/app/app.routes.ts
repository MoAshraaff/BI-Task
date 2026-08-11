import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    path: 'products',
    loadComponent: () => import('./features/products/product-list/product-list').then((m) => m.ProductList),
    canActivate: [authGuard]
  },
  {
    path: 'products/new',
    loadComponent: () => import('./features/products/product-form/product-form').then((m) => m.ProductForm),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'products/:id/edit',
    loadComponent: () => import('./features/products/product-form/product-form').then((m) => m.ProductForm),
    canActivate: [authGuard, adminGuard]
  },
  { path: '**', redirectTo: 'products' }
];
