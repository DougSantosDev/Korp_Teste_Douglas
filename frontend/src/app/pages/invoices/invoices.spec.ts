import { ChangeDetectorRef } from '@angular/core';
import { of } from 'rxjs';
import { InvoiceService } from '../../services/invoice.service';
import { ProductService } from '../../services/product.service';
import { Invoices } from './invoices';

describe('Invoices', () => {
  const invoiceService = {
    getAll: () => of([]),
    create: () => of({}),
    print: () => of({}),
  } as unknown as InvoiceService;

  const productService = {
    getAll: () => of([]),
  } as unknown as ProductService;

  const changeDetectorRef = {
    detectChanges: () => undefined,
  } as unknown as ChangeDetectorRef;

  it('merges repeated products into a single invoice item', () => {
    const component = new Invoices(invoiceService, productService, changeDetectorRef);
    component.products = [{ id: 1, code: 'P1', description: 'Produto', stockQuantity: 10 }];

    component.selectedProductId = 1;
    component.selectedQuantity = 2;
    component.addItem();
    component.selectedProductId = 1;
    component.selectedQuantity = 3;
    component.addItem();

    expect(component.items).toEqual([{ productId: 1, quantity: 5 }]);
  });

  it('does not add a quantity above the available stock', () => {
    const component = new Invoices(invoiceService, productService, changeDetectorRef);
    component.products = [{ id: 1, code: 'P1', description: 'Produto', stockQuantity: 1 }];
    component.selectedProductId = 1;
    component.selectedQuantity = 2;

    component.addItem();

    expect(component.items).toHaveLength(0);
    expect(component.errorMessage).toContain('saldo disponível');
  });

  it('translates API status values for the interface', () => {
    const component = new Invoices(invoiceService, productService, changeDetectorRef);

    expect(component.getStatusLabel('Open')).toBe('Aberta');
    expect(component.getStatusLabel('Closed')).toBe('Fechada');
  });
});
