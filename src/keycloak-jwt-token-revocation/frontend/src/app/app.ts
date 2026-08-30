import { Component } from '@angular/core';
import { ProductsPageComponent } from './features/products/products-page.component';

@Component({
  selector: 'app-root',
  imports: [ProductsPageComponent],
  template: '<app-products-page />',
})
export class App {}
