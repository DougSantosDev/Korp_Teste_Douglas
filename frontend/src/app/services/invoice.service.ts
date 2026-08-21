import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Invoice, InvoiceItem } from '../models/invoice';

@Injectable({
  providedIn: 'root',
})
export class InvoiceService {
  private readonly apiUrl = 'http://localhost:5227/api/invoices';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.apiUrl);
  }

  create(items: InvoiceItem[]): Observable<Invoice> {
    return this.http.post<Invoice>(this.apiUrl, {
      items,
    });
  }

  print(id: number): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/${id}/print`, {});
  }
}
