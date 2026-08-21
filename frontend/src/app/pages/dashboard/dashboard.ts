import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ProductService } from '../../services/product.service';
import { InvoiceService } from '../../services/invoice.service';
import { Product } from '../../models/product';
import { Invoice } from '../../models/invoice';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  products: Product[] = [];
  invoices: Invoice[] = [];

  loading = true;
  errorMessage = '';

  constructor(
    private productService: ProductService,
    private invoiceService: InvoiceService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getAll().subscribe({
      next: (products) => {
        this.products = products;
        this.loadInvoices();
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os dados do estoque.';
        this.loading = false;
      }
    });
  }

  private loadInvoices(): void {
    this.invoiceService.getAll().subscribe({
      next: (invoices) => {
        this.invoices = invoices;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar as notas fiscais.';
        this.loading = false;
      }
    });
  }

  get totalProducts(): number {
    return this.products.length;
  }

  get openInvoices(): number {
    return this.invoices.filter(
      invoice => invoice.status === 'Open'
    ).length;
  }

  get closedInvoices(): number {
    return this.invoices.filter(
      invoice => invoice.status === 'Closed'
    ).length;
  }
}