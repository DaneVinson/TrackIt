import { Component, Input, Output, EventEmitter, OnChanges, SimpleChange } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';

import { CategoriesService } from '../../shared/categories.service';

@Component({
    selector: 'ti-category-details',
    templateUrl: './category-details.component.html'
})
export class CategoryDetailsComponent implements OnChanges {
    constructor(private categoriesService: CategoriesService, formBuilder: FormBuilder) {
        this.viewForm = formBuilder.group({
            'description': ['', Validators.required],
            'id': { value: '', disabled: true },
            'units': ['', Validators.required]
        });
        this.description = this.viewForm.controls['description'];
        this.id = this.viewForm.controls['id'];
        this.units = this.viewForm.controls['units'];
    }

    @Input() category: CategoryEntity;
    @Input() originalCategory: CategoryEntity;
    @Output() categoryChanged = new EventEmitter<CategoryEntity>();
    @Output() setBusy = new EventEmitter<boolean>();

    accountId: string;
    deleted: boolean;
    description: AbstractControl;
    id: AbstractControl;
    new: boolean;
    units: AbstractControl;
    viewForm: FormGroup;

    // cancel any changes and restore category to its original state
    cancel(): void {
        this.deleted = false;
        this.originalCategory.deleted = this.originalCategory.new;  // cancelling new => delete
        this.originalCategory.dirty = false;
        this.originalCategory.valid = true;
        this.originalCategory.dataPoints = [];
        this.resetToCategory(this.originalCategory);
    }

    // target of the delete action to mark the current category for deletion
    delete(): void {
        this.category.deleted = true;
        this.deleted = true;
        this.viewForm.markAsDirty();
    }

    // called on change to inputs
    ngOnChanges(): void {
        // if touched send update to parent and reset form to pristine and untouched
        var dirty = this.viewForm.touched || this.viewForm.dirty;
        if (dirty) {
            this.categoryChanged.emit({
                accountId: this.accountId,
                dataPoints: null,
                deleted: this.deleted,
                description: this.description.value,
                dirty: dirty,
                id: this.id.value,
                new: this.new,
                units: this.units.value,
                valid: this.viewForm.valid
            });
            this.viewForm.markAsPristine();
            this.viewForm.markAsUntouched();
        }

        this.accountId = this.category.accountId;
        this.deleted = this.category.deleted;
        this.description.setValue(this.category.description);
        this.id.setValue(this.category.id);
        this.new = this.category.new;
        this.units.setValue(this.category.units);
        if (this.category.dirty) {
            this.viewForm.markAsDirty();
        }
    }

    reset(): void {
        this.viewForm.markAsPristine();
        this.viewForm.markAsUntouched();
    }

    private resetToCategory(category: CategoryEntity): void {
        // form
        this.viewForm.markAsPristine();
        this.viewForm.markAsUntouched();
        this.description.setValue(category.description);
        this.id.setValue(category.id);
        this.units.setValue(category.units);

        // category
        this.category.accountId = category.accountId;
        this.category.dataPoints = category.dataPoints;
        this.category.deleted = category.deleted;
        this.category.description = category.description;
        this.category.dirty = category.dirty;
        this.category.id = category.id;
        this.category.new = category.new;
        this.category.units = category.units;
        this.category.valid = category.valid;

        // emit changes to parent
        this.categoryChanged.emit(this.category);
    }

    save(): void {
        this.setBusy.emit(true);
        this.category.description = this.description.value;
        this.category.units = this.units.value;
        if (this.category.deleted) {
            this.categoriesService.deleteCategory(this.category.id).subscribe(
                // success
                (data) => {
                    // deleted and not dirty triggers removal
                    this.category.deleted = true;
                    this.category.dirty = false;
                    this.setBusy.emit(false);   // categoryChanged.emit removes category which disrupts setBusy if run after
                    this.categoryChanged.emit(this.category);
                },
                // error
                (error) => {
                },
                // complete
                () => {
                    this.setBusy.emit(false);
                });
        }
        else {
            this.categoriesService.saveCategory(this.category).subscribe(
                // success
                (data) => {
                    // the Observable<Category> is returning an object array with object[0] as the category
                    var category: Category = data[0];
                    this.categoryChanged.emit({
                        accountId: category.accountId,
                        dataPoints: null,
                        deleted: false,
                        description: category.description,
                        dirty: false,
                        id: category.id,
                        new: false,
                        units: category.units,
                        valid: true
                    });
                    this.viewForm.markAsPristine();
                    this.viewForm.markAsUntouched();
                },
                // error
                (error) => {
                },
                // complete
                () => {
                    this.setBusy.emit(false);
                });
        }
    }
}
