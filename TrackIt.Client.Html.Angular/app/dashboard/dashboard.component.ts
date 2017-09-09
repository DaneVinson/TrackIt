import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';

import { CategoriesService } from './shared/categories.service';
import { CategoryComponent } from './category/category.component';
import { CategoryListComponent } from './category-list/category-list.component';

@Component({
    providers: [CategoriesService],
    selector: 'ti-dashboard',
    templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
    constructor(private categoriesService: CategoriesService, private changeDetector: ChangeDetectorRef) {
        this.busy = false;
        this.sideNavActive = true;
    }

    busy: boolean;
    categories: CategoryEntity[];
    categoriesInfo: CategoryEntity[];
    selectedCategory: CategoryEntity;
    selectedCategoryInfo: CategoryEntity;
    sideNavActive: boolean;


    @ViewChild(CategoryComponent)
    private categoryComponent: CategoryComponent;
    @ViewChild(CategoryListComponent)
    private categoryListComponent: CategoryListComponent;

    // child output for category component
    categoryEdited(categoryEntity: CategoryEntity): void {
        // super ugly method to allow delete flow through children components
        if (categoryEntity.deleted && (categoryEntity.new || !categoryEntity.dirty)) {
            this.selectedCategory.new = categoryEntity.new || !categoryEntity.dirty;
            this.deleteCategory();
        }
        else {
            var category = this.categories.find(c => c.id == categoryEntity.id);
            category.accountId = categoryEntity.accountId;
            category.deleted = categoryEntity.deleted;
            category.description = categoryEntity.description;
            category.dirty = categoryEntity.dirty;
            category.id = categoryEntity.id;
            category.units = categoryEntity.units;
            category.valid = categoryEntity.valid;
            if (!categoryEntity.dirty) {
                category.new = false;
            }

            var categoryInfo = this.categoriesInfo.find(c => c.id == categoryEntity.id);
            categoryInfo.accountId = categoryEntity.accountId;
            categoryInfo.deleted = categoryEntity.deleted;
            categoryInfo.dirty = categoryEntity.dirty;
            categoryInfo.id = categoryEntity.id;
            categoryInfo.valid = categoryEntity.valid;
            if (!categoryEntity.dirty) {
                categoryInfo.description = categoryEntity.description;
                categoryInfo.new = false;
                categoryInfo.units = categoryEntity.units;
            }
        }

        this.changeDetector.detectChanges();
    }

    // child output for category component
    dataPointsChanged(dataPoints: DataPoint[]): void {
        if (dataPoints && dataPoints.length > 0) {
            var category = this.categories.find(c => c.id == dataPoints[0].categoryId);
            var categoryInfo = this.categoriesInfo.find(c => c.id == dataPoints[0].categoryId);
            if (category) {
                if (!dataPoints[0].id) {
                    category.dataPoints = [];
                    categoryInfo.dataPoints = [];
                }
                else {
                    category.dataPoints = dataPoints;
                    categoryInfo.dataPoints = new Array(dataPoints.length);
                }
            }
        }
    }

    // child output for category list component
    selectCategory(categoryEntity: CategoryEntity) {
        if (categoryEntity) {
            var selectedCategory = this.categories.find(c => c.id == categoryEntity.id);
            if (selectedCategory) {
                this.selectedCategory = selectedCategory;
            }
            var selectedCategoryInfo = this.categoriesInfo.find(c => c.id == categoryEntity.id);
            if (selectedCategoryInfo) {
                this.selectedCategoryInfo = selectedCategoryInfo;
            }
        }
        else {
            this.selectedCategory = null;
            this.selectedCategoryInfo = null;
        }
        if (this.selectedCategory && this.selectedCategory.new) {
            this.categoryComponent.setDisplayMode('details');
        }
        this.changeDetector.detectChanges();
    }

    // child output for category list component
    setBusy(busy: boolean): void {
        this.busy = busy;
    }


    // mark the selected category as deleted unless new then remove
    deleteCategory(): void {
        if (this.selectedCategory.new) {
            var index = this.categories.findIndex(c => c.id == this.selectedCategoryInfo.id);
            this.categories.splice(index, 1);
            this.categoriesInfo.splice(index, 1);
            this.selectCategory(null);
            this.categoryListComponent.toggleSelected(null);
            this.categoryComponent.reset();
        }
        else {
            this.categoryComponent.delete();
        }
    }

    // TODO: hacky, this should be refactored to use a pipe or filter
    getCaption(): string {
        if (this.selectedCategoryInfo) {
            var caption = this.selectedCategoryInfo.description;
            if (this.selectedCategoryInfo.units) {
                caption += ' (' + this.selectedCategoryInfo.units + ')';;
            }
            return caption;
        }
        else {
            return 'Select or add a category';
        }
    }

    private getCategories(): void {
        this.busy = true;
        this.categoriesService.getCategories().subscribe(
            // success
            (categories) => {
                this.categories = this.mapToEntities(categories);
                this.categoriesInfo = [];
                this.categories.forEach((c) => {
                    this.categoriesInfo.push({
                        accountId: c.accountId,
                        dataPoints: c.dataPoints,
                        description: c.description,
                        deleted: c.deleted,
                        dirty: false,
                        id: c.id,
                        new: c.new,
                        units: c.units,
                        valid: c.valid
                    })
                });
            },
            // error
            (error) => { },
            // complete
            () => {
                this.selectCategory(null);
                this.categoryListComponent.toggleSelected(null);
                this.categoryComponent.reset();
                this.busy = false;
            });
    }

    private mapToEntities(categories: Category[]): CategoryEntity[] {
        var entities: CategoryEntity[] = [];
        categories.forEach(c => {
            entities.push({
                accountId: c.accountId,
                dataPoints: c.dataPoints,
                deleted: false,
                description: c.description,
                dirty: false,
                id: c.id,
                new: false,
                units: c.units,
                valid: true
            });
        });
        return entities;
    }

    newCategory(): void {
        var newCount = this.categoriesInfo.filter(c => c.description.startsWith('NewCategory')).length;
        var description = 'NewCategory' + ++newCount;

        var category = this.categoriesService.newCategory();
        category.description = description
        category.id = newCount.toString();
        this.categories.splice(newCount - 1, 0, category);

        var categoryInfo = this.categoriesService.newCategory();
        categoryInfo.description = description;
        categoryInfo.id = newCount.toString();
        this.categoriesInfo.splice(newCount - 1, 0, categoryInfo);

        this.categoryListComponent.toggleSelected(categoryInfo.id);
        this.categoryComponent.setDisplayMode('details');
    }

    ngOnInit(): void {
        this.getCategories();
    }

    toggleSideNav(): void {
        this.sideNavActive = !this.sideNavActive;
    }
}
