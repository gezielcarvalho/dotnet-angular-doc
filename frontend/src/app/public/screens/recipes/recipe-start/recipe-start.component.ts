import { Component, Input, OnInit } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { Recipe } from 'src/app/shared/models/recipe.model';
import { RecipeService } from 'src/app/shared/services/recipe.service';

@Component({
    standalone: true,
    imports: [MatMenuModule],
    selector: 'app-recipe-start',
    templateUrl: './recipe-start.component.html',
    styleUrls: ['./recipe-start.component.css'],
})
export class RecipeStartComponent implements OnInit {
    @Input() recipe: Recipe | undefined;
    constructor(private service: RecipeService) {}
    ngOnInit() {
        // TODO
    }
}
