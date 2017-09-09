import { Injectable } from '@angular/core';
import { Headers, Http, RequestOptionsArgs, Response } from '@angular/http';

// rxjs
import { Observable } from 'rxjs/Observable';
import {Observer} from 'rxjs/Observer';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { AppConfig } from '../../app.config';
import { OAuthService } from '../../shared/oauth.service';

@Injectable()
export class CategoriesService {
    constructor(private appConfig: AppConfig, private http: Http, private oAuthService: OAuthService) {
        this.categoriesUrl = appConfig.getConfig('apiUri') + '/api/categories';
    }

    categoriesUrl: string;

    deleteCategory(id: string): Observable<boolean> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        return this.http.delete(this.categoriesUrl + '/' + id, requestOptions)
            .map((response: Response) => {
                return response.json();
            })
            .catch(this.handleError);
    }

    getCategories(): Observable<Category[]> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        return this.http.get(this.categoriesUrl, requestOptions)
            .map((response: Response) => {
                return response.json();
            })
            .catch(this.handleError);
    }

    private handleError(error: any) {
        console.error(error);
        return Observable.throw(error.json().message || 'An unknown error has occured.');
    }

    private mapEntityToCategory(entity: CategoryEntity): Category {
        return {
            accountId: entity.accountId,
            dataPoints: entity.dataPoints,
            description: entity.description,
            id: entity.id,
            units: entity.units
        };
    }

    newCategory(): CategoryEntity {
        return {
            accountId: '',
            dataPoints: [],
            deleted: false,
            description: '',
            dirty: true,
            id: '',
            new: true,
            units: '',
            valid: false
        };
    }

    saveCategory(categoryEntity: CategoryEntity): Observable<Category> {
        var requestOptions = {
            headers: this.oAuthService.getHeaders()
        }

        // post
        if (categoryEntity.new) {
            return this.http
                .post(this.categoriesUrl, this.mapEntityToCategory(categoryEntity), requestOptions)
                .map((response: Response) => {
                    return response.json();
                })
                .catch(this.handleError);
        }

        // put
        else {
            return this.http
                .put(this.categoriesUrl, this.mapEntityToCategory(categoryEntity), requestOptions)
                .map((response: Response) => {
                    return response.json();
                })
                .catch(this.handleError);
        }
    }
}
