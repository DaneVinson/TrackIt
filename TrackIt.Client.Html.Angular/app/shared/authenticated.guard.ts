import { CanActivate } from "@angular/router";
import { Injectable } from "@angular/core";

import { OAuthService } from './oauth.service';

@Injectable()
export class AuthenticatedGuard implements CanActivate {

    constructor(private oAuthService: OAuthService) {
    }

    canActivate() {
        if (this.oAuthService.getAccessToken()) {
            return true;
        }
        else {
            return false;
        }
    }
}