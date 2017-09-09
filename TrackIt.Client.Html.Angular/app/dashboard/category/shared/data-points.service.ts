import { Injectable } from '@angular/core';
import { Http, Response, URLSearchParams } from '@angular/http';

// rxjs
import { Observable } from 'rxjs/Observable';
import {Observer} from 'rxjs/Observer';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { AppConfig } from '../../../app.config';
import { OAuthService } from '../../../shared/oauth.service';

@Injectable()
export class DataPointsService {
    constructor(private appConfig: AppConfig, private http: Http, private oAuthService: OAuthService) {
        var host = appConfig.getConfig('apiUri');
        this.categoriesUrl = host + '/api/categories';
        this.dataPointsUrl = host + '/api/datapoints';
    }

    private categoriesUrl: string;
    private dataPointsUrl: string;

    addDataPoints(dataPoints: DataPoint[]): Observable<DataPoint[]> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        return this.http.post(this.dataPointsUrl, dataPoints, requestOptions)
            .map((response: Response) => {
                return response.json();
            })
            .catch(this.handleError);
    }

    deleteDataPoint(id: string): Observable<boolean> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        return this.http.delete(this.dataPointsUrl + '/' + id, requestOptions)
            .map((response: Response) => {
                return true;
            })
            .catch(this.handleError);
    }

    getDataPoints(categoryId: string, dateRange: DateRange): Observable<Category> {
        let params: URLSearchParams = new URLSearchParams();
        // hack-fu with dates to coerce them into the desired form for the api call
        params.set('from', new Date(dateRange.from.toString()).toISOString().substring(0, 10));
        params.set('to', new Date(dateRange.to.toString()).toISOString().substring(0, 10));

        var requestOptions = {
            headers: this.oAuthService.getHeaders(),
            search: params
        }

        return this.http.get(this.categoriesUrl + '/' + categoryId, requestOptions)
            .map((response: Response) => {
                return response.json();
            })
            .catch(this.handleError);
    }

    getUtcDateString(date: Date): string {
        if (!date) {
            return '';
        }
        var testDate: Date = date;
        if (isNaN(testDate.getTime())) {
            testDate = new Date(date.toString());
        }
        return (testDate.getUTCMonth() + 1) + '/' + testDate.getUTCDate() + '/' + testDate.getUTCFullYear();
    }

    updateDataPoints(dataPoints: DataPoint[]): Observable<DataPoint[]> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        return this.http.put(this.dataPointsUrl, dataPoints, requestOptions)
            .map((response: Response) => {
                return response.json();
            })
            .catch(this.handleError);
    }

    private handleError(error: any) {
        console.error(error);
        return Observable.throw(error.json().message || 'An unknown error has occured.');
    }
}
