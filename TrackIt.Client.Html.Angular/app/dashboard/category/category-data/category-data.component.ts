import { Component, Input, Output, EventEmitter, OnChanges, SimpleChange } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';

import { DataPointsService } from '../shared/data-points.service';
import { dateValidator } from '../../../shared/date-validator.directive';

@Component({
    selector: 'ti-category-data',
    templateUrl: './category-data.component.html'
})
export class CategoryDataComponent implements OnChanges {
    constructor(private dataPointsService: DataPointsService, formBuilder: FormBuilder) {
        this.viewForm = formBuilder.group({
            'from': ['', Validators.compose([Validators.required, dateValidator()])],
            'id': { value: '', disabled: true },
            'stamp': ['', Validators.compose([Validators.required, dateValidator()])],
            'to': ['', Validators.compose([Validators.required, dateValidator()])],
            'value': [0]
        });
        this.dataPoint = null;
        this.deleted = false;
        this.new = false;

        // reactive forms
        this.from = this.viewForm.controls['from'];
        this.id = this.viewForm.controls['id'];
        this.stamp = this.viewForm.controls['stamp'];
        this.to = this.viewForm.controls['to'];
        this.value = this.viewForm.controls['value'];
    }

    // angular Input/Output
    @Input() categoryId: string;
    @Input() dataPoints: DataPoint[];
    @Output() setBusy = new EventEmitter<boolean>();
    @Output() updateDataPoints = new EventEmitter<DataPoint[]>();

    // reactive forms
    viewForm: FormGroup;
    id: AbstractControl;
    from: AbstractControl;
    stamp: AbstractControl;
    to: AbstractControl;
    value: AbstractControl;

    dataPoint: DataPoint;
    deleted: boolean;
    new: boolean;

    cancel(): void {
        this.dataPoint = null;
        this.setDataPoint(null);
    }

    private addDataPoint(): void {
        this.setBusy.emit(true);
        var dataPoint: DataPoint = {
            categoryId: this.categoryId,
            id: this.dataPoint.id,
            stamp: this.stamp.value,
            value: this.value.value
        };
        this.dataPointsService.addDataPoints([dataPoint]).subscribe(
            (result) => {
                this.setBusy.emit(false);
                this.cancel();
                if (this.from.valid && this.to.valid) {
                    this.search();
                }
            },
            (error) => {
                this.setBusy.emit(false);
                this.cancel();
            });
    }

    delete(): void {
        this.deleted = true;
        this.stamp.disable();
        this.value.disable();
    }

    private deleteDataPoint(id: string): void {
        this.setBusy.emit(true);
        this.dataPointsService.deleteDataPoint(id).subscribe(
            (result) => {
                this.dataPoints.splice(this.dataPoints.findIndex(d => d.id == id), 1);
                this.setBusy.emit(false);
                this.cancel();
            },
            (error) => {
                this.setBusy.emit(false);
                this.cancel();
            });
    }

    editDataPoint(id: string): void {
        this.setDataPoint(this.dataPoints.find(d => d.id == id));
    }

    // TODO: hacky, this should be refactored to use a pipe or filter
    getEditCaption(): string {
        return this.new ? 'New Data Point' : 'Edit Data Point';
    }

    hasDataPoints(): boolean {
        return this.dataPoints && this.dataPoints.length > 0;
    }

    newDataPoint(): void {
        var dataPoint = {
            categoryId: this.categoryId,
            id: 'newId',
            stamp: null,
            value: 0
        }
        this.setDataPoint(dataPoint, true);
    }

    // called on change to inputs
    ngOnChanges(): void {
        if (this.dataPoints && this.dataPoints.length > 0) {
            var tempDateFrom = new Date(this.dataPoints[0].stamp.toString());
            var tempDateTo = new Date(this.dataPoints[this.dataPoints.length - 1].stamp.toString());
            this.from.setValue(this.dataPointsService.getUtcDateString(tempDateFrom));
            this.to.setValue(this.dataPointsService.getUtcDateString(tempDateTo));
        }
        else {
            this.from.setValue('');
            this.to.setValue('');
        }
    }

    private setDataPoint(dataPoint: DataPoint, isNew: boolean = false): void {
        this.deleted = false;
        this.new = isNew;
        if (dataPoint) {
            this.dataPoint = dataPoint;
            this.id.setValue(dataPoint.id);
            this.stamp.setValue(dataPoint.stamp);
            this.value.setValue(dataPoint.value);
        }
        else {
            this.dataPoint = null;
            this.id.setValue('');
            this.stamp.setValue(null);
            this.value.setValue(0);
        }

        // reset reactive forms elements as needed
        if (this.stamp.dirty) {
            this.stamp.markAsPristine();
        }
        if (this.value.dirty) {
            this.value.markAsPristine();
        }
        if (!this.stamp.enabled) {
            this.stamp.enable();
        }
        if (!this.value.enabled) {
            this.value.enable();
        }
    }

    save(): void {
        if (this.deleted) {
            this.deleteDataPoint(this.dataPoint.id);
        }
        else if (this.new) {
            this.addDataPoint();
        }
        else {
            this.updateDataPoint();
        }
    }

    search(): void {
        this.setBusy.emit(true);
        var dateRange: DateRange = {
            from: this.from.value,
            to: this.to.value
        };
        this.dataPointsService.getDataPoints(this.categoryId, dateRange).subscribe(
            // success
            (category) => {
                this.dataPoints = category.dataPoints;
                this.updateDataPoints.emit(this.dataPoints);
                this.setBusy.emit(false);
            },
            // error
            (error) => {
                this.dataPoints = [];
                this.updateDataPoints.emit(this.dataPoints);
                this.setBusy.emit(false);
            });
    }

    private updateDataPoint(): void {
        this.setBusy.emit(false);
        var dataPoint: DataPoint = {
            categoryId: this.categoryId,
            id: this.dataPoint.id,
            stamp: this.stamp.value,
            value: this.value.value
        };
        this.dataPointsService.updateDataPoints([dataPoint]).subscribe(
            (result) => {
                this.setBusy.emit(false);
                this.cancel();
                if (this.from.valid && this.to.valid) {
                    this.search();
                }
            },
            (error) => {
                this.setBusy.emit(false);
                this.cancel();
            });
    }
}
