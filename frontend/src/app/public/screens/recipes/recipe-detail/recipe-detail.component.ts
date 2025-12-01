
import { Component, Input, OnInit } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { ActivatedRoute } from '@angular/router';
import { Recipe } from 'src/app/shared/models/recipe.model';
import { RecipeService } from 'src/app/shared/services/recipe.service';

@Component({
    imports: [MatMenuModule],
    selector: 'app-recipe-detail',
    templateUrl: './recipe-detail.component.html',
    styleUrls: ['./recipe-detail.component.css']
})
export class RecipeDetailComponent implements OnInit {
    @Input() recipe: Recipe | undefined;
    constructor(
        private service: RecipeService,
        private route: ActivatedRoute,
    ) {}
    ngOnInit() {
        this.route.params.subscribe(params => {
            this.recipe = this.service.getRecipe(+params['id']);
        });
    }

    onAddToShoppingList() {
        this.service.addIngredientsToShoppingList(this.recipe!.ingredients);
    }

    onEditRecipe() {
        console.log('RecipeDetailComponent.onEditRecipe');
    }

    onDeleteRecipe() {
        console.log('RecipeDetailComponent.onDeleteRecipe');
    }
}
