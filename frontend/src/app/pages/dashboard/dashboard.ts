import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize, forkJoin } from 'rxjs';

import { ProductService } from '../../services/product.service';
import { InvoiceService } from '../../services/invoice.service';
import { Product } from '../../models/product';
import { Invoice } from '../../models/invoice';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  products: Product[] = [];
  invoices: Invoice[] = [];

  loading = true;
  errorMessage = '';

  constructor(
    private productService: ProductService,
    private invoiceService: InvoiceService,
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      products: this.productService.getAll(),
      invoices: this.invoiceService.getAll(),
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: ({ products, invoices }) => {
          this.products = products;
          this.invoices = invoices;
        },
        error: () => {
          this.errorMessage = 'Não foi possível carregar os dados do dashboard.';
        },
      });
  }

  get totalProducts(): number {
    return this.products.length;
  }

  get openInvoices(): number {
    return this.invoices.filter((invoice) => invoice.status === 'Open').length;
  }

  get closedInvoices(): number {
    return this.invoices.filter((invoice) => invoice.status === 'Closed').length;
  }
}
