import { Component, EventEmitter, Input, OnChanges, Output, SimpleChange } from '@angular/core';

import { CategoriesService } from '../shared/categories.service';

@Component({
    selector: 'ti-category-list',
    templateUrl: './category-list.component.html'
})
export class CategoryListComponent {
    constructor(private categoriesService: CategoriesService) {
        this.categories = [];
        this.selectedCategory = null;
    }

    @Input() categories: CategoryEntity[];
    @Input() selectedCategory: CategoryEntity;
    @Output() selectionChanged = new EventEmitter<CategoryEntity>();

    isSelected(id: string): boolean {
        return id && this.selectedCategory && id == this.selectedCategory.id;
    }

    toggleSelected(id: string): void {
        var newCategory: CategoryEntity;
        if (this.categories && (!this.selectedCategory || id != this.selectedCategory.id)) {
            newCategory = this.categories.find(c => c.id == id);
            if (newCategory) {
                this.selectedCategory = newCategory;
            }
        }
        if (!newCategory) {
            this.selectedCategory = null;
        }
        this.selectionChanged.emit(this.selectedCategory);
    }
}
