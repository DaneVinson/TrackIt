import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-http-client';
import {AureliaConfiguration} from 'aurelia-configuration';

import {AuthService} from './auth-service';

@inject(AuthService, AureliaConfiguration)
export class CategoryService {
    private apiUri: string;
    public categories: CategoryEntity[];
    public categoryEntities: CategoryEntity[];
    public isBusy: boolean;
    private newCount: number;

    constructor(private authService: AuthService, private config: AureliaConfiguration) {
        this.isBusy = false;
        this.newCount = 0;
        this.apiUri = config.get('apiUri') + 'categories';
        this.refreshCategories();
    }

    addNewCategory(): string {
        let count = this.newCount++;
        let newId = 'NewCategory' + count;
        let entity: CategoryEntity = {
            accountId: this.authService.accountId,
            dataPoints: [],
            deleted: false,
            description: newId,
            dirty: false,
            id: newId,
            new: true,
            units: '',
            valid: false
        }
        this.categoryEntities.push(entity);
        this.categories.push(JSON.parse(JSON.stringify(entity)));

        return entity.id;
    }

    cancelCategoryChanges(id: string): void {
        let category = this.categories.find(c => c.id == id);
        let categoryEntity = this.categoryEntities.find(c => c.id == id);
        if (category && categoryEntity) {
            if (category.new) {
                let index = this.categories.indexOf(category);
                this.categories.splice(index, 1);
                this.categoryEntities.splice(index, 1);
            }
            else {
                categoryEntity.accountId = category.accountId;
                categoryEntity.dataPoints = [];
                categoryEntity.deleted = false;
                categoryEntity.description = category.description;
                categoryEntity.dirty = false;
                categoryEntity.id = category.id;
                categoryEntity.new = false;
                categoryEntity.units = category.units;
                categoryEntity.valid = true;
                category.deleted = false;
                category.dirty = false;
                category.deleted = false;
            }
        }
    }

    private entityFromCategory(category: Category): CategoryEntity {
        return {
            accountId: category.accountId,
            dataPoints: category.dataPoints,
            deleted: false,
            description: category.description,
            dirty: false,
            id: category.id,
            new: false,
            units: category.units,
            valid: true // assume the input category was valid
        };
    }

    public refreshCategories(): void {
        let client = new HttpClient()
        client.configure(c => {
            c.withBaseUrl(this.apiUri);
            c.withHeader('Authorization', 'bearer ' + this.authService.token);
        });
        this.isBusy = true;
        client.get(this.apiUri)
            .then(response => {
                this.categories = null;
                this.categoryEntities = null;
                if (response.isSuccess) {
                    this.categories = [];
                    this.categoryEntities = [];
                    let categories: Category[] = response.content;
                    if (categories && categories.length > 0) {
                        categories.forEach(c => {
                            let entity = this.entityFromCategory(c);
                            this.categoryEntities.push(entity);
                            this.categories.push(JSON.parse(JSON.stringify(entity)));
                        });
                    }
                }
                if (!this.categories) {
                    this.categories = [];
                    this.categoryEntities = []
                }
                this.newCount = 0;
                this.isBusy = false;
            })
            .catch(reason => {
                this.categories = [];
                this.categoryEntities = [];
                this.newCount = 0;
                this.isBusy = false;
            });
    }
}