import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { Product } from '../../../../shared/models/Product';
import { ProductService } from 'src/app/shared/services/product.service';

@Component({
    standalone: true,
    selector: 'app-product-details',
    templateUrl: './product-details.component.html',
})
export class ProductDetailsComponent implements OnInit, OnDestroy {
    product: Product = {} as Product;
    paramsSubscription: Subscription = {} as Subscription;

    constructor(
        private route: ActivatedRoute,
        private itemService: ProductService,
    ) {}

    ngOnInit(): void {
        this.paramsSubscription = this.route.params.subscribe(params => {
            const id = Number(params['id']);
            if (!isNaN(id)) {
                this.itemService
                    .getProduct(id)
                    .subscribe(product => (this.product = product));
            }
        });
    }

    ngOnDestroy(): void {
        this.paramsSubscription.unsubscribe();
    }

    addToCart(): void {}
}
