import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { InvoiceService } from '../../services/invoice.service';
import { ProductService } from '../../services/product.service';
import { Invoice, InvoiceItem } from '../../models/invoice';
import { Product } from '../../models/product';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './invoices.html',
  styleUrl: './invoices.scss'
})
export class Invoices implements OnInit {
  invoices: Invoice[] = [];
  products: Product[] = [];

  items: InvoiceItem[] = [];

  selectedProductId = 0;
  selectedQuantity = 1;

  loading = false;
  submitting = false;
  printingId: number | null = null;

  errorMessage = '';
  successMessage = '';

  constructor(
    private invoiceService: InvoiceService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
    this.loadProducts();
  }

  loadInvoices(): void {
    this.loading = true;

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

  loadProducts(): void {
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products = products;
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os produtos.';
      }
    });
  }

  addItem(): void {
    if (this.selectedProductId <= 0 || this.selectedQuantity <= 0) {
      return;
    }

    this.items.push({
      productId: this.selectedProductId,
      quantity: this.selectedQuantity
    });

    this.selectedProductId = 0;
    this.selectedQuantity = 1;
  }

  removeItem(index: number): void {
    this.items.splice(index, 1);
  }

  createInvoice(): void {
    if (this.items.length === 0) {
      this.errorMessage = 'Adicione pelo menos um produto à nota.';
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.invoiceService.create(this.items).subscribe({
      next: () => {
        this.successMessage = 'Nota fiscal criada com sucesso.';
        this.items = [];
        this.submitting = false;
        this.loadInvoices();
      },
      error: () => {
        this.errorMessage = 'Não foi possível criar a nota fiscal.';
        this.submitting = false;
      }
    });
  }

  printInvoice(invoice: Invoice): void {
    this.printingId = invoice.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.invoiceService.print(invoice.id).subscribe({
      next: () => {
        this.successMessage = `Nota ${invoice.number} impressa com sucesso.`;
        this.printingId = null;
        this.loadInvoices();
        this.loadProducts();
      },
      error: (error) => {
        this.printingId = null;

        if (error.status === 409) {
          this.errorMessage =
            'A nota não pode ser impressa ou o estoque é insuficiente.';
          return;
        }

        if (error.status === 503) {
          this.errorMessage =
            'Serviço de estoque temporariamente indisponível.';
          return;
        }

        this.errorMessage = 'Não foi possível imprimir a nota.';
      }
    });
  }

  getProductDescription(productId: number): string {
    return this.products.find(p => p.id === productId)?.description
      ?? `Produto ${productId}`;
  }
}