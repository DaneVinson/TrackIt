import { Routes, RouterModule } from '@angular/router';

import { AboutComponent } from './about/about.component';
import { AuthenticatedGuard } from './shared/authenticated.guard';
import { ContactComponent } from './contact/contact.component';
import { DashboardComponent } from './dashboard/dashboard.component';

const appRoutes: Routes = [
    { path: '', component: AboutComponent },
    { path: 'about', component: AboutComponent },
    { path: 'contact', component: ContactComponent },
    { path: 'dashboard', component: DashboardComponent, canActivate: [AuthenticatedGuard] }
];

export const routing = RouterModule.forRoot(appRoutes);
