import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-list',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe, DatePipe],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  protected readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(false);
  protected readonly deletingId = signal<number | null>(null);
  protected readonly categories = signal<string[]>([]);

  protected readonly totalCount = computed(() => this.products().length);
  protected readonly lowStockCount = computed(() => this.products().filter((p) => p.stock < 10).length);
  protected readonly totalValue = computed(() => this.products().reduce((sum, p) => sum + p.price * p.stock, 0));

  protected readonly filterForm = this.fb.nonNullable.group({
    search: [''],
    category: [''],
    sort: ['name-asc']
  });

  ngOnInit(): void {
    this.loadCategories();
    this.search();
  }

  /** Builds an OData $filter/$orderby query from the form and re-runs GET /odata/Products. */
  search(): void {
    const { search, category, sort } = this.filterForm.getRawValue();

    const filters: string[] = [];
    if (search.trim()) {
      filters.push(`contains(tolower(Name), '${this.escapeODataString(search.trim().toLowerCase())}')`);
    }
    if (category) {
      filters.push(`Category eq '${this.escapeODataString(category)}'`);
    }

    this.loading.set(true);
    this.productService
      .query({
        filter: filters.length ? filters.join(' and ') : undefined,
        orderby: this.mapSort(sort)
      })
      .subscribe({
        next: (response) => {
          this.products.set(response.value);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  resetFilters(): void {
    this.filterForm.reset({ search: '', category: '', sort: 'name-asc' });
    this.search();
  }

  delete(product: Product): void {
    if (!confirm(`Delete "${product.name}"? This cannot be undone.`)) {
      return;
    }

    this.deletingId.set(product.id);
    this.productService.delete(product.id).subscribe({
      next: () => {
        this.notifications.success(`"${product.name}" was deleted.`);
        this.deletingId.set(null);
        this.search();
      },
      error: () => this.deletingId.set(null)
    });
  }

  private loadCategories(): void {
    this.productService.getAll().subscribe({
      next: (products) => {
        const unique = Array.from(new Set(products.map((p) => p.category).filter((c) => !!c)));
        this.categories.set(unique.sort());
      }
    });
  }

  private mapSort(sort: string): string {
    switch (sort) {
      case 'price-asc':
        return 'Price asc';
      case 'price-desc':
        return 'Price desc';
      case 'name-desc':
        return 'Name desc';
      case 'newest':
        return 'CreatedAt desc';
      default:
        return 'Name asc';
    }
  }

  private escapeODataString(value: string): string {
    return value.replace(/'/g, "''");
  }
}
