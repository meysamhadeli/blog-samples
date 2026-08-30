import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { KeycloakService } from '../../core/services/keycloak.service';
import { Product } from './models/product.model';
import { ProductsService } from './services/products.service';

@Component({
  selector: 'app-products-page',
  imports: [DecimalPipe],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.css',
})
export class ProductsPageComponent implements OnInit {
  private readonly productsService = inject(ProductsService);
  private readonly keycloak = inject(KeycloakService);
  protected products: Product[] = [];
  protected error = '';
  protected loading = false;

  public ngOnInit(): void {
    void this.loadProducts();
  }

  protected get username(): string {
    return this.keycloak.tokenParsed?.['preferred_username'] as string ?? 'authenticated user';
  }

  protected async loadProducts(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      this.products = await this.productsService.getProducts();
    } catch (error) {
      this.error = error instanceof Error ? error.message : 'Request failed';
    } finally {
      this.loading = false;
    }
  }

  protected logout(): Promise<void> {
    return this.keycloak.logout();
  }
}
