import { Component } from '@angular/core';
import { RecipeListComponent } from './recipe-list/recipe-list.component';
import { RecipeDetailComponent } from './recipe-detail/recipe-detail.component';

import { RecipeStartComponent } from './recipe-start/recipe-start.component';
import { RouterModule } from '@angular/router';

@Component({
    imports: [
    RecipeListComponent,
    RecipeDetailComponent,
    RecipeStartComponent,
    RouterModule
],
    selector: 'app-recipes',
    templateUrl: './recipes.component.html'
})
export class RecipesComponent {
    constructor() {}
}
