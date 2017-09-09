import {Aurelia} from 'aurelia-framework';
import 'font-awesome/css/font-awesome.min.css';
import 'purecss/build/pure-min.css';
import 'purecss/build/grids-responsive-min.css';
import '../css/side-menu.css';
import '../css/app.css';
import environment from './environment';

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .plugin('aurelia-configuration', config => {
      config.setEnvironments({
          development: ['localhost'],
          production: ['trackit-client-aurelia.azurewebsites.net']
        });
      })
    .feature('resources');

  if (environment.debug) {
    aurelia.use.developmentLogging();
  }

  // if (environment.testing) {
  //   aurelia.use.plugin('aurelia-testing');
  // }

  aurelia.start()
    .then(() => aurelia.setRoot('app', document.body))
    // contents of the pure-ui.js file for the purecss side-menu template, must be executed after document load
    .then(() => {
      var layout   = document.getElementById('layout'),
          menu     = document.getElementById('menu'),
          menuLink = document.getElementById('menuLink'),
          content  = document.getElementById('main');

      function toggleClass(element, className) {
        var classes = element.className.split(/\s+/),
            length = classes.length,
            i = 0;

        for(; i < length; i++) {
          if (classes[i] === className) {
            classes.splice(i, 1);
            break;
          }
        }
        // The className is not found
        if (length === classes.length) {
            classes.push(className);
        }

        element.className = classes.join(' ');
      }

      function toggleAll(e) {
        var active = 'active';
        e.preventDefault();
        toggleClass(layout, active);
        toggleClass(menu, active);
        toggleClass(menuLink, active);
      }

      menuLink.onclick = function (e) {
        toggleAll(e);
      };

      content.onclick = function(e) {
        if (menu.className.indexOf('active') !== -1) {
          toggleAll(e);
        }
      };
  });
}
