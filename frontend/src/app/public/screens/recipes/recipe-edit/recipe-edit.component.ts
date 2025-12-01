import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { ActivatedRoute } from '@angular/router';
import { Recipe } from 'src/app/shared/models/recipe.model';
import { RecipeService } from 'src/app/shared/services/recipe.service';

@Component({
    imports: [MatMenuModule, CommonModule],
    selector: 'app-recipe-edit',
    templateUrl: './recipe-edit.component.html',
    styleUrls: ['./recipe-edit.component.css']
})
export class RecipeEditComponent implements OnInit {
    recipe: Recipe | undefined;
    isEditing = false;
    constructor(
        private service: RecipeService,
        private route: ActivatedRoute,
    ) {}
    ngOnInit() {
        this.route.params.subscribe(params => {
            if (!params['id']) {
                return;
            }
            this.isEditing = true;
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
