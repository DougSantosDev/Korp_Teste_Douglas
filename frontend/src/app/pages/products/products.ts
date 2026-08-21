import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
})
export class Products implements OnInit {
  products: Product[] = [];
  loading = false;
  submitting = false;
  errorMessage = '';
  successMessage = '';

  newProduct = {
    code: '',
    description: '',
    stockQuantity: 0,
  };

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService
      .getAll()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (products) => {
          this.products = products;
        },
        error: () => {
          this.errorMessage = 'Não foi possível carregar os produtos.';
        },
      });
  }

  createProduct(): void {
    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService
      .create({
        code: this.newProduct.code.trim(),
        description: this.newProduct.description.trim(),
        stockQuantity: this.newProduct.stockQuantity,
      })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => {
          this.successMessage = 'Produto cadastrado com sucesso.';

          this.newProduct = {
            code: '',
            description: '',
            stockQuantity: 0,
          };

          this.loadProducts();
        },
        error: (error) => {
          if (error.status === 409) {
            this.errorMessage = 'Já existe um produto com este código.';
            return;
          }

          this.errorMessage = 'Não foi possível cadastrar o produto.';
        },
      });
  }
}
