import { Component, OnDestroy, OnInit } from '@angular/core';
import { ShoppingEditComponent } from './shopping-edit/shopping-edit.component';
import { Ingredient } from 'src/app/shared/models/ingredients.model';
import { CommonModule } from '@angular/common';
import { ShoppingListService } from 'src/app/shared/services/shopping-list.service';

@Component({
    imports: [ShoppingEditComponent, CommonModule],
    selector: 'app-shopping-list',
    templateUrl: './shopping-list.component.html'
})
export class ShoppingListComponent implements OnInit, OnDestroy {
    ingredients: Ingredient[] = [];
    ingredientsSubscription: any;
    constructor(private service: ShoppingListService) {}
    ngOnDestroy(): void {
        this.ingredientsSubscription.unsubscribe();
    }
    ngOnInit() {
        this.ingredients = this.service.getIngredients();
        this.ingredientsSubscription =
            this.service.ingredientsChanged.subscribe(
                (ingredients: Ingredient[]) => {
                    this.ingredients = ingredients;
                },
            );
    }
}
