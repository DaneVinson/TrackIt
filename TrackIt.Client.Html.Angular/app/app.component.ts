import { Component } from '@angular/core';

import { AppConfig } from './app.config';
import { OAuthService } from './shared/oauth.service';

@Component({
    providers: [OAuthService],
    selector: 'ti-app',
    templateUrl: './app.component.html'
})
export class AppComponent {
    constructor(private appConfig: AppConfig, private oAuthService: OAuthService) {
        this.authenticationCaption = 'Sign In';

        this.oAuthService.aadb2c = true;
        this.oAuthService.clientId = appConfig.getConfig('authAppId');
        this.oAuthService.loginUrl = appConfig.getConfig('oauth2Uri') + 'authorize'
        this.oAuthService.logoutRedirectUri = window.location.origin + '/about';
        this.oAuthService.logoutUrl = appConfig.getConfig('oauth2Uri') + 'logout';        
        this.oAuthService.oidc = true;  // set to true, to receive also an id_token via OpenId Connect (OIDC) in addition to the OAuth2-based access_token
        this.oAuthService.redirectUri = window.location.origin + '/dashboard';  // login redirect
        this.oAuthService.scope = 'openid profile';
        this.oAuthService.aadb2cPolicy = appConfig.getConfig('aadPolicy');
        
        if (this.oAuthService.getAccessToken()) {
            this.authenticationCaption = 'Sign Out';
        }

        this.oAuthService.tryLogin({
            onTokenReceived: context => {
                if (context && context.idClaims && context.idClaims.name) {
                    this.authenticationCaption = 'Sign Out ' + context.idClaims.name;
                }
                else {
                    this.authenticationCaption = 'Sign Out';
                }
            }
        });
    }

    authenticationCaption: string;

    signInOut(): void {
        var token = this.oAuthService.getAccessToken();
        if (token) {
            this.oAuthService.logOut();
        }
        else {
            this.oAuthService.initImplicitFlow();
        }
    }
}