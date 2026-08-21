import { Routes } from '@angular/router';
import { Products } from './pages/products/products';
import { Invoices } from './pages/invoices/invoices';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  },
  {
    path: 'products',
    component: Products
  },
  {
    path: 'invoices',
    component: Invoices
  }
];