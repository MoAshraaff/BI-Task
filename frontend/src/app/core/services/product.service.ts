import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { CreateProductRequest, ODataResponse, Product, ProductODataQuery, UpdateProductRequest } from '../models/product.model';

/** Raw shape of a Product entity as returned by /odata/Products: PascalCase, matching the EDM model. */
interface ODataProduct {
  Id: number;
  Name: string;
  Description: string;
  Category: string;
  Price: number;
  Stock: number;
  CreatedAt: string;
  UpdatedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly restBase = `${API_BASE_URL}/api/products`;
  private readonly odataBase = `${API_BASE_URL}/odata/Products`;

  constructor(private readonly http: HttpClient) {}

  // ---- Plain REST CRUD (System.Text.Json camelCase DTOs) ----

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.restBase);
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.restBase}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.restBase, request);
  }

  update(id: number, request: UpdateProductRequest): Observable<void> {
    return this.http.put<void>(`${this.restBase}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.restBase}/${id}`);
  }

  // ---- OData querying ----

  /**
   * GET /odata/Products?$filter=...&$select=...&$orderby=...&$top=...
   * OData serializes entities using the EDM model's own property names (PascalCase - Id, Name,
   * Price, ...), not System.Text.Json's camelCase policy used by the plain REST DTOs. Normalized
   * to the same camelCase `Product` shape here so the rest of the app doesn't care which endpoint
   * the data came from.
   */
  query(options: ProductODataQuery): Observable<ODataResponse<Product>> {
    let params = new HttpParams();
    if (options.filter) {
      params = params.set('$filter', options.filter);
    }
    if (options.select) {
      params = params.set('$select', options.select);
    }
    if (options.orderby) {
      params = params.set('$orderby', options.orderby);
    }
    if (options.top) {
      params = params.set('$top', options.top);
    }

    return this.http.get<ODataResponse<ODataProduct>>(this.odataBase, { params }).pipe(
      map((response) => ({
        ...response,
        value: response.value.map(toProduct)
      }))
    );
  }
}

function toProduct(raw: ODataProduct): Product {
  return {
    id: raw.Id,
    name: raw.Name,
    description: raw.Description,
    category: raw.Category,
    price: raw.Price,
    stock: raw.Stock,
    createdAt: raw.CreatedAt,
    updatedAt: raw.UpdatedAt
  };
}
