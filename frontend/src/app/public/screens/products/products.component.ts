import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Product } from '../../../shared/models/Product';
import { ProductService } from 'src/app/shared/services/product.service';
import { StoreService } from 'src/app/shared/services/store.service';
import { RouterModule } from '@angular/router';
import { FilterComponent } from 'src/app/shared/components/filter/filter.component';
@Component({
    imports: [FormsModule, CommonModule, RouterModule, FilterComponent],
    selector: 'app-products',
    templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
    products: Product[] = [];
    constructor(
        private productService: ProductService,
        public storeService: StoreService,
    ) {}
    ngOnInit(): void {
        this.storeService.pageSizeChanges$.subscribe(() => {
            this.storeService.page = 1;
            this.getProducts();
        });

        this.getProducts();
    }
    getProducts(): void {
        this.productService
            .getProducts(this.storeService.page, this.storeService.pageSize, {
                categories: [],
                name: '',
            })
            .subscribe(itemPayload => {
                this.storeService.products = itemPayload.products;
                this.storeService.count = itemPayload.count;
            });
    }

    onPageChange(newPage: number): void {
        this.storeService.page = newPage;
        this.getProducts();
    }

    onPageSizeChange(): void {
        this.storeService._pageSizeSubject.next(this.storeService.pageSize);
    }
}
