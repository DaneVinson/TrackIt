import { inject, computedFrom } from 'aurelia-framework';
import { Router } from 'aurelia-router';
import {EventAggregator} from 'aurelia-event-aggregator';
import { AuthService } from '../auth-service';
import { CategoryService } from '../category-service';
import { CategoryChange } from '../shared/category-change';

@inject(AuthService, CategoryService, EventAggregator, Router)
export class Menu {
    public authenticated: boolean;
    public selectedCategory: CategoryEntity;

    constructor(private authService: AuthService, private categoryService: CategoryService, private eventAggregator: EventAggregator, private router: Router) {
        this.authenticated = !!this.authService.token;
        this.selectedCategory = null;
        this.eventAggregator.subscribe(CategoryChange, message => this.categoryChange(message.category.id));
    }

    public categoryChange(targetId: string) {
        if (!targetId) {
            return;
        }

        if (this.selectedCategory && this.selectedCategory.id != targetId) {
            let entity = this.categoryService.categoryEntities.find(c => c.id == this.selectedCategory.id);
            let category = this.categoryService.categories.find(c => c.id == this.selectedCategory.id);
            if (category && entity) {
                category.deleted = entity.deleted;
                category.dirty = entity.dirty;
                category.new = entity.new;
                category.valid = entity.valid;
            }
        }
        let entity = this.categoryService.categoryEntities.find(c => c.id == targetId);
        let category = this.categoryService.categories.find(c => c.id == targetId);
        if (category && entity) {
            category.deleted = entity.deleted;
            category.dirty = entity.dirty;
            category.new = entity.new;
            category.valid = entity.valid;
        }
        this.selectedCategory = category;
    }

    public deleteCategory(): void {
        let routeParts = this.router.currentInstruction.fragment.split('/');
        if (!routeParts || routeParts.length == 0) {
            return;
        }
        let id = routeParts[routeParts.length - 1];
        let category = this.categoryService.categories.find(c => c.id == id);
        let entity = this.categoryService.categoryEntities.find(c => c.id == id);
        if (category) {
            if (category.new) {
                this.categoryService.cancelCategoryChanges(category.id);
                this.router.navigate('dashboard');
            }
            else {
                category.deleted = true;
                entity.deleted = true;
            }
        }
    }

    public newCategory(): void {
        let id = this.categoryService.addNewCategory();
        this.router.navigate('dashboard/'+ id);
    }

    public refreshCategories(): void {
        this.categoryService.refreshCategories();
        this.selectedCategory = null;
        this.router.navigate('dashboard');
    }

    public signIn(): void {
        this.authService.signIn();
    }

    public signOut(): void {
        this.authService.signOut();
    }
}