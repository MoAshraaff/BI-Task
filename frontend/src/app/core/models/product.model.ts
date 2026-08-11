export interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  stock: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  category: string;
  price: number;
  stock: number;
}

export type UpdateProductRequest = CreateProductRequest;

/** Shape of an OData collection response: { "@odata.context": "...", "value": [...] } */
export interface ODataResponse<T> {
  '@odata.context': string;
  '@odata.count'?: number;
  value: T[];
}

/** Optional OData query options for GET /odata/Products. Every field is optional. */
export interface ProductODataQuery {
  filter?: string;
  select?: string;
  orderby?: string;
  top?: number;
}
