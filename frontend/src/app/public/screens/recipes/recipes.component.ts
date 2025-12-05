import { Component } from '@angular/core';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
// RecipeDetailComponent and RecipeStartComponent are not used directly in this template; they are child route targets
import { RouterModule } from '@angular/router';

@Component({
    standalone: true,
    imports: [RecipeListComponent, RouterModule],
    selector: 'app-recipes',
    templateUrl: './recipes.component.html',
})
export class RecipesComponent {
    constructor() {}
}
