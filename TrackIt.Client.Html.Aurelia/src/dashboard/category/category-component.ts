import { BindingEngine, inject, computedFrom } from 'aurelia-framework';
import { Router } from 'aurelia-router';
import { EventAggregator } from 'aurelia-event-aggregator';
import { CategoryChange } from '../../shared/category-change';
import { CategoryService } from '../../category-service';

@inject(CategoryService, BindingEngine, EventAggregator, Router)
export class CategoryComponent {
    private category: CategoryEntity;
    public categoryEntity: CategoryEntity;
    public displayModes: string[];
    public selectedDisplayMode: string;
    public title: string;

    constructor(private categoryService: CategoryService, private bindingEngine: BindingEngine, private eventAggregator: EventAggregator, private router: Router) {
        this.displayModes = [ 'Details', 'Data', 'Charting' ];
        this.setDisplay(this.displayModes[0]);
    }

    @computedFrom('categoryEntity', 'categoryEntity.deleted', 'categoryEntity.new')
    get canDelete(): boolean {
        if (!this.categoryEntity) {
            return false;
        }
        return !this.categoryEntity.deleted && !this.categoryEntity.new
    }  

    @computedFrom('categoryEntity', 'categoryEntity.description', 'categoryEntity.units', 'categoryEntity.deleted')
    get canSave(): boolean {
        if (!this.categoryEntity) {
            return false;
        }

        // set dirty
        this.categoryEntity.dirty = this.category.description != this.categoryEntity.description ||
                                    this.category.units != this.categoryEntity.units ||
                                    this.categoryEntity.deleted;

        // set valid
        var valid = true;
        if (!this.categoryEntity.description || !this.categoryEntity.units) {
            valid = false;
        }
        this.categoryEntity.valid = valid;                         

        // can save if deleted or both dirty and valid
        return this.categoryEntity.deleted || (this.categoryEntity.dirty && this.categoryEntity.valid);
    }  
    
    activate(params, routeConfig) {
        var categoryEntity = this.categoryService.categoryEntities.find(c => c.id == params.id);
        if (categoryEntity) {
            this.categoryEntity = categoryEntity;
            this.category = this.categoryService.categories.find(c => c.id == this.categoryEntity.id);
            this.title = this.category.description
            if (this.category.units) {
                this.title += ' (' + this.category.units + ')';
            }
        }
        else {
            this.router.navigate('dashboard');
        }
        this.eventAggregator.publish(new CategoryChange(this.category));
    }

    cancel() {
        var isNew = this.categoryEntity.new === true;
        this.categoryService.cancelCategoryChanges(this.categoryEntity.id);
        if (isNew) {
            this.router.navigate('dashboard');
        }
    }

    delete() {
        this.category.deleted = true;
        this.categoryEntity.deleted = true;
        this.eventAggregator.publish(new CategoryChange(this.category));
    }

    save() {
    }

    setDisplay(displayMode: string){
        var selected = this.displayModes.find(d => d == displayMode);
        if (selected){
            this.selectedDisplayMode = displayMode;
        }
        else {
            this.selectedDisplayMode = this.displayModes[0];
        }
    }
}
