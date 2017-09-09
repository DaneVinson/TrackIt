import { Router, RouterConfiguration } from 'aurelia-router';
import { inject } from 'aurelia-framework';
import { AuthService } from './auth-service';
import { CategoryService } from './category-service';

@inject(AuthService, CategoryService)
export class App {
  router: Router;
  
  constructor(private authService: AuthService, private categoryService: CategoryService) {
    var hash = window.location.hash
    if (hash) {
      var authQueryFragment = hash.substr(1);
      if (authQueryFragment) {
        this.authService.setToken(authQueryFragment);
        window.location.hash = '';
        this.categoryService.refreshCategories();
      }
    }
  }

  configureRouter(config: RouterConfiguration, router: Router) {
    config.map([
      { route: '', moduleId: 'dashboard/about/about', name: 'about'},
      { route: 'about', moduleId: 'dashboard/about/about', name: 'about'},
      { route: 'contact', moduleId: 'dashboard/contact/contact', name: 'contact'},
      { route: 'dashboard', moduleId: 'dashboard/category/empty', name: 'empty'},
      { route: 'dashboard/:id', moduleId: 'dashboard/category/category-component', name:'dashboard' }
    ]);

    this.router = router;
  }
}