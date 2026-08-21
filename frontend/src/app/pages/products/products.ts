import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss'
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
    stockQuantity: 0
  };

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.productService.getAll().subscribe({
      next: (products) => {
        this.products = products;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar os produtos.';
        this.loading = false;
      }
    });
  }

  createProduct(): void {
    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.productService.create(this.newProduct).subscribe({
      next: () => {
        this.successMessage = 'Produto cadastrado com sucesso.';
        this.submitting = false;

        this.newProduct = {
          code: '',
          description: '',
          stockQuantity: 0
        };

        this.loadProducts();
      },
      error: (error) => {
        this.submitting = false;

        if (error.status === 409) {
          this.errorMessage = 'Já existe um produto com este código.';
          return;
        }

        this.errorMessage = 'Não foi possível cadastrar o produto.';
      }
    });
  }
}