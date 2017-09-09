import {inject} from 'aurelia-framework';
import {AureliaConfiguration} from "aurelia-configuration";

@inject(AureliaConfiguration)
export class AuthService {
    private readonly id: string;
    private readonly origin: string;
    private readonly policy: string;
    private readonly redirectUri: string;
    private readonly rootAutUri: string;
    accountId: string;
    token: string;

    constructor(private config: AureliaConfiguration) {
        this.id = config.get('authAppId') || '';
        this.origin = window.location.origin || '';
        this.policy = config.get('aadPolicy') || '';
        this.redirectUri = this.origin;
        this.rootAutUri = config.get('oauth2Uri') || '';

        this.accountId = '';
        this.token = '';
    }

    private getNonce(): string {
        return 'defaultNonce';
    }

    public getQueryArguments(queryString: string): string[] {
        if (!queryString) {
            return [];
        }
        let decodedQueryString = decodeURI(queryString);
        let parts = decodedQueryString.split('&');
        if (!parts || parts.length == 0) {
            return [];
        }
        return parts;
    }

    public getQueryArgumentValue(key: string, args: string[]): string {
        // handle invalid input
        if (!key || !args || args.length == 0) {
            return '';
        }

        // search for the key's value
        for (let i = 0; i < args.length; i++)
        {
            if (!!args[i]) {
                let parts = args[i].split('=');
                if (parts && parts.length == 2 && key.toLowerCase() == parts[0].toLowerCase())
                {
                    return parts[1];
                }
            }
        }

        // default fallback
        return '';
    }

    public setToken(queryString: string): void {
        if (!queryString) {
            this.accountId = '';
            this.token = '';
            return;
        }
        let args = queryString.split('&');
        if (!args || args.length == 0) {
            this.accountId = '';
            this.token = '';
            return;
        }

        // search for id_token
        for (let i = 0; i < args.length; i++)
        {
            if (!!args[i]) {
                let parts = args[i].split('=');
                if (parts && parts.length == 2 && parts[0].toLowerCase() == 'id_token')
                {
                    this.accountId = '';
                    this.token = parts[1];
                    return;
                }
            }
        }

        // default fallback
        this.accountId = '';
        this.token = '';
    }

    signIn(): void {
        location.href =
            this.rootAutUri + 'authorize?' +
            'p=' + this.policy + 
            '&client_id=' + this.id + 
            '&nonce=' + this.getNonce() +
            '&redirect_uri=' + encodeURIComponent(this.redirectUri) +
            '&scope=openid&response_type=id_token&prompt=login';
    }

    signOut(): void {
        location.href =
            this.rootAutUri + 'logout?' +
            'p=' + this.policy + 
            '&post_logout_redirect_uri=' + encodeURIComponent(this.origin);
    }
}
