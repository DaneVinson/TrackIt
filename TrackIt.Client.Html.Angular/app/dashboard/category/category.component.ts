import { Component, EventEmitter, Input, OnChanges, Output, SimpleChange, ViewChild } from '@angular/core';

import { CategoriesService } from '../shared/categories.service';
import { DataPointsService } from './shared/data-points.service';
import { CategoryChartingComponent } from './category-charting/category-charting.component';
import { CategoryDataComponent } from './category-data/category-data.component';
import { CategoryDetailsComponent } from './category-details/category-details.component';

@Component({
    providers: [DataPointsService],
    selector: 'ti-category',
    templateUrl: './category.component.html'
})
export class CategoryComponent {
    constructor(private categoriesService: CategoriesService, private dataPointsService: DataPointsService) {
        this.displayModes = ['details', 'data', 'charting'];
        this.displayMode = this.displayModes[0];
    }

    @Input() category: CategoryEntity;
    @Input() originalCategory: CategoryEntity;
    @Output() categoryEdited = new EventEmitter<CategoryEntity>();
    @Output() updateDataPoints = new EventEmitter<DataPoint[]>();
    @Output() setCategoryBusy = new EventEmitter<boolean>();

    @ViewChild(CategoryChartingComponent)
    private categoryChartingComponent: CategoryChartingComponent;
    @ViewChild(CategoryDataComponent)
    private categoryDataComponent: CategoryDataComponent;
    @ViewChild(CategoryDetailsComponent)
    private categoryDetailsComponent: CategoryDetailsComponent;

    displayMode: string;
    displayModes: string[];


    // child component output
    categoryChanged(categoryEntity: CategoryEntity): void {
        this.categoryEdited.emit(categoryEntity);
    }

    // child component output
    dataPointsChanged(dataPoints: DataPoint[]): void {
        this.updateDataPoints.emit(dataPoints);
    }

    // child component output
    setBusy(busy: boolean): void {
        this.setCategoryBusy.emit(busy);
    }


    // mark the category as deleted
    delete(): void {
        this.categoryDetailsComponent.delete();
    }

    // reset self and children
    reset() {
        if (this.categoryDetailsComponent) {
            this.categoryDetailsComponent.reset();
        }
        this.setDisplayMode(this.displayModes[0]);
    }

    // set the input display mode
    setDisplayMode(mode: string): void {
        var mode = this.displayModes.find(m => m == mode);
        if (mode) {
            this.displayMode = mode;
        }
        else {
            this.displayMode = this.displayModes[0];
        }
    }
}
