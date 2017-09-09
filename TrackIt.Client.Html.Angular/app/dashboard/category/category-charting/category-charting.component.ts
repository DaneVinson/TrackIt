import { Component, Input, OnChanges } from '@angular/core';

import { DataPointsService } from '../shared/data-points.service';

@Component({
    selector: 'ti-category-charting',
    templateUrl: './category-charting.component.html'
})
export class CategoryChartingComponent implements OnChanges {
    constructor(private dataPointsService: DataPointsService) {
        this.chartOptions = null;
    }

    @Input() categoryLabel: string;
    @Input() dataPoints: DataPoint[];

    chartOptions: any;

    // get the chart series from the current dataPoints
    private getChartSeries(): any[][] {
        var data: any[][] = [];
        if (!this.dataPoints || this.dataPoints.length == 0) {
            return data;
        }
        this.dataPoints.forEach(d => {
            var tempDate: Date = new Date(d.stamp.toString());
            data.push([Date.UTC(tempDate.getUTCFullYear(), tempDate.getUTCMonth(), tempDate.getUTCDate()), d.value]);
        });
        return data;
    }

    private getChartTitle(): string {
        var from = new Date(this.dataPoints[0].stamp.toString());
        var to = new Date(this.dataPoints[this.dataPoints.length - 1].stamp.toString());
        return this.dataPointsService.getUtcDateString(from) + ' - ' + this.dataPointsService.getUtcDateString(to);
    }

    // called on change to inputs
    ngOnChanges(): void {
        if (!this.dataPoints || this.dataPoints.length == 0) {
            this.chartOptions = null;
        }
        else {
            this.chartOptions = {
                chart: {
                    spacing: [10, 0, 10, 0]
                },
                plotOptions: {
                    series: {
                        animation: false
                    },
                    spline: {
                        animation: false
                    }
                },
                series: [{
                    data: this.getChartSeries(),
                    name: this.categoryLabel,
                    showInLegend: false
                }],
                title: { text: this.getChartTitle() },
                xAxis: {
                    type: 'datetime',
                    dateTimeLabelFormats: {
                        day: '%m/%d/%Y',
                        month: '%m/%Y'
                    },
                    title: {
                        text: 'Date'
                    }
                },
                yAxis: {
                    title: {
                        text: this.categoryLabel
                    }
                }
            };
        }
    }
}
