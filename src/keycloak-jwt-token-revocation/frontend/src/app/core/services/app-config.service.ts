import { Injectable } from '@angular/core';

export interface RuntimeConfig {
  apiUrl: string;
  keycloakUrl: string;
  keycloakRealm: string;
  keycloakClientId: string;
}

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private config?: RuntimeConfig;
  private loadPromise?: Promise<void>;

  public load(): Promise<void> {
    this.loadPromise ??= fetch('/config.json', { cache: 'no-store' })
      .then(async (response) => {
        if (!response.ok) throw new Error(`Runtime configuration failed: ${response.status}`);
        this.config = this.validate(await response.json() as Partial<RuntimeConfig>);
      });
    return this.loadPromise;
  }

  public get value(): RuntimeConfig {
    if (!this.config) throw new Error('Runtime configuration has not loaded');
    return this.config;
  }

  private validate(config: Partial<RuntimeConfig>): RuntimeConfig {
    const required = ['apiUrl', 'keycloakUrl', 'keycloakRealm', 'keycloakClientId'] as const;
    for (const key of required) {
      if (!config[key] || typeof config[key] !== 'string') throw new Error(`Runtime configuration is missing ${key}`);
    }
    return config as RuntimeConfig;
  }
}
