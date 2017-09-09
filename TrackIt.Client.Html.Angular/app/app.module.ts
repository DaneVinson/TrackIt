import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { ChartModule } from '../node_modules/angular2-highcharts';

import { AboutComponent } from './about/about.component';
import { AppComponent } from './app.component';
import { AppConfig } from './app.config';
import { AuthenticatedGuard } from './shared/authenticated.guard';
import { CategoryChartingComponent } from './dashboard/category/category-charting/category-charting.component';
import { CategoryComponent } from './dashboard/category/category.component';
import { CategoryDataComponent } from './dashboard/category/category-data/category-data.component';
import { CategoryDetailsComponent } from './dashboard/category/category-details/category-details.component';
import { CategoryListComponent } from './dashboard/category-list/category-list.component';
import { ContactComponent } from './contact/contact.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { DateValidatorDirective } from './shared/date-validator.directive';
import { OAuthService } from './shared/oauth.service';
import { routing } from './app.routing';

@NgModule({
    imports: [
        BrowserModule,
        ChartModule.forRoot(require('highcharts')),
        FormsModule,
        HttpModule,
        ReactiveFormsModule,
        routing
    ],
    declarations: [
        AboutComponent,
        AppComponent,
        CategoryChartingComponent,
        CategoryComponent,
        CategoryDataComponent,
        CategoryDetailsComponent,
        CategoryListComponent,
        ContactComponent,
        DashboardComponent,
        DateValidatorDirective
    ],
    providers: [
        AppConfig,
        AuthenticatedGuard,
        OAuthService,
        { provide: APP_INITIALIZER, useFactory: (config: AppConfig) => () => config.load(), deps: [AppConfig], multi: true }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}