import { Injectable, inject } from '@angular/core';
import Keycloak, { KeycloakInstance } from 'keycloak-js';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class KeycloakService {
  private readonly config = inject(AppConfigService);
  private client?: KeycloakInstance;

  public async initialize(): Promise<void> {
    await this.config.load();
    const settings = this.config.value;
    this.client = new Keycloak({
      url: settings.keycloakUrl,
      realm: settings.keycloakRealm,
      clientId: settings.keycloakClientId,
    });
    await this.client.init({
      onLoad: 'login-required',
      flow: 'standard',
      pkceMethod: 'S256',
      checkLoginIframe: false,
    });
  }

  public get token(): string | undefined { return this.client?.token; }
  public get tokenParsed(): KeycloakInstance['tokenParsed'] { return this.client?.tokenParsed; }

  public async logout(): Promise<void> {
    const token = this.client?.token;
    const settings = this.config.value;
    if (token) {
      const response = await fetch(`${settings.keycloakUrl}/realms/${settings.keycloakRealm}/protocol/openid-connect/revoke`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ client_id: settings.keycloakClientId, token, token_type_hint: 'access_token' }),
        keepalive: true,
      });
      await response.text();
      if (!response.ok) throw new Error(`Token revocation failed: ${response.status}`);
    }
    await this.client?.logout({ redirectUri: window.location.origin });
  }
}
