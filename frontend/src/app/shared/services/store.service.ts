import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { Product } from '../models/Product';
import { Subject } from 'rxjs/internal/Subject';
import { Filter } from '../models/Filter';

@Injectable({
    providedIn: 'root',
})
export class StoreService {
    private readonly _products = new BehaviorSubject<Product[]>([]);
    readonly products$ = this._products.asObservable();
    get products(): Product[] {
        return this._products.getValue();
    }
    set products(val: Product[]) {
        this._products.next(val);
    }
    private readonly _page = new BehaviorSubject<number>(1);
    readonly page$ = this._page.asObservable();

    get page(): number {
        return this._page.getValue();
    }

    set page(val: number) {
        this._page.next(val);
    }

    public pageSize: number = 3;
    public readonly _pageSizeSubject = new Subject<number>();
    public pageSizeChanges$ = this._pageSizeSubject.asObservable();

    private readonly _filter = new BehaviorSubject<Filter>({
        name: '',
        categories: [],
    });

    readonly filter$ = this._filter.asObservable();
    get filter(): Filter {
        return this._filter.getValue();
    }
    set filter(val: Filter) {
        this._filter.next(val);
    }

    private readonly _count = new BehaviorSubject<number>(1);
    readonly count$ = this._count.asObservable();

    get count(): number {
        return this._count.getValue();
    }

    set count(val: number) {
        this._count.next(val);
    }
    constructor() {}
}
