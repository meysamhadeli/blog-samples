import { Injectable, inject } from '@angular/core';
import { AppConfigService } from '../../../core/services/app-config.service';
import { KeycloakService } from '../../../core/services/keycloak.service';
import { Product } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private readonly config = inject(AppConfigService);
  private readonly keycloak = inject(KeycloakService);

  public async getProducts(): Promise<Product[]> {
    const response = await fetch(`${this.config.value.apiUrl}/api/v1/catalogs/products`, {
      headers: { Authorization: `Bearer ${this.keycloak.token}` },
    });
    if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
    return await response.json() as Product[];
  }
}
