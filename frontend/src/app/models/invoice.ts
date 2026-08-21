export interface InvoiceItem {
  id?: number;
  invoiceId?: number;
  productId: number;
  quantity: number;
}

export interface Invoice {
  id: number;
  number: number;
  status: 'Open' | 'Closed';
  createdAt: string;
  items: InvoiceItem[];
}
