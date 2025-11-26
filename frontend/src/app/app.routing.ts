import { Route } from '@angular/router';
import { HomeComponent } from './public/screens/home/home.component';
import { AuthGuard } from './shared/services/auth-guard.service';

export const routes: Route[] = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full',
    },
    {
        path: 'login',
        loadComponent: () =>
            import('./public/screens/login/login.component').then(
                m => m.LoginComponent,
            ),
    },
    {
        path: 'products',
        loadComponent: () =>
            import('./public/screens/products/products.component').then(
                m => m.ProductsComponent,
            ),
    },
    {
        path: 'products/:id',
        loadComponent: () =>
            import(
                './public/screens/products/product-details/product-details.component'
            ).then(m => m.ProductDetailsComponent),
    },
    {
        path: 'td-form',
        loadComponent: () =>
            import(
                './public/screens/template-form/template-form.component'
            ).then(m => m.TemplateFormComponent),
    },
    {
        path: 'reactive-form',
        loadComponent: () =>
            import(
                './public/screens/reactive-form/reactive-form.component'
            ).then(m => m.ReactiveFormComponent),
    },
    {
        path: 'recipes',
        loadComponent: () =>
            import('./public/screens/recipes/recipes.component').then(
                m => m.RecipesComponent,
            ),
        children: [
            {
                path: '',
                loadComponent: () =>
                    import(
                        './public/screens/recipes/recipe-start/recipe-start.component'
                    ).then(m => m.RecipeStartComponent),
            },
            {
                path: 'new',
                // canActivate: [AuthGuard],
                loadComponent: () =>
                    import(
                        './public/screens/recipes/recipe-edit/recipe-edit.component'
                    ).then(m => m.RecipeEditComponent),
            },
            {
                path: ':id',
                loadComponent: () =>
                    import(
                        './public/screens/recipes/recipe-detail/recipe-detail.component'
                    ).then(m => m.RecipeDetailComponent),
            },
            {
                path: ':id/edit',
                // canActivate: [AuthGuard],
                loadComponent: () =>
                    import(
                        './public/screens/recipes/recipe-edit/recipe-edit.component'
                    ).then(m => m.RecipeEditComponent),
            },
        ],
    },
    {
        path: 'shopping-list',
        loadComponent: () =>
            import(
                './public/screens/recipes/shopping-list/shopping-list.component'
            ).then(m => m.ShoppingListComponent),
    },
    {
        path: 'reserved',
        canActivate: [AuthGuard],
        loadComponent: () =>
            import('./public/screens/reserved/reserved.component').then(
                m => m.ReservedComponent,
            ),
    },
    {
        path: '**',
        redirectTo: '/',
    },
];
