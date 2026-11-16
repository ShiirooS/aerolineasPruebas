import { InjectionToken } from '@angular/core';

/**
 * Configuración que cambia entre ambientes (dev, qa, preprod, prod).
 *
 * No se compila dentro del bundle: se lee de `config.json` al arrancar (ver
 * `main.ts`). Así el mismo build se promueve de un ambiente al siguiente y en
 * cada despliegue solo se reemplaza `config.json` por el de `config/<ambiente>/`.
 */
export type AppEnvironment = 'dev' | 'qa' | 'preprod' | 'prod';

export interface AppConfig {
  readonly environment: AppEnvironment;
  /** URL base de la API, sin barra final. */
  readonly apiBaseUrl: string;
}

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG');

const ENVIRONMENTS: readonly AppEnvironment[] = ['dev', 'qa', 'preprod', 'prod'];

function isAppEnvironment(value: unknown): value is AppEnvironment {
  return typeof value === 'string' && (ENVIRONMENTS as readonly string[]).includes(value);
}

export function parseAppConfig(raw: unknown): AppConfig {
  if (typeof raw !== 'object' || raw === null) {
    throw new Error('config.json debe ser un objeto JSON.');
  }
  const candidate = raw as Record<string, unknown>;

  const environment = candidate['environment'];
  if (!isAppEnvironment(environment)) {
    throw new Error(`config.json: "environment" debe ser uno de ${ENVIRONMENTS.join(', ')}.`);
  }

  const apiBaseUrl = candidate['apiBaseUrl'];
  if (typeof apiBaseUrl !== 'string' || !URL.canParse(apiBaseUrl)) {
    throw new Error('config.json: "apiBaseUrl" debe ser una URL absoluta.');
  }

  return { environment, apiBaseUrl: apiBaseUrl.replace(/\/+$/, '') };
}

export async function loadAppConfig(
  fetchConfig: typeof fetch = fetch,
  url = 'config.json',
): Promise<AppConfig> {
  const response = await fetchConfig(url, { cache: 'no-store' });
  if (!response.ok) {
    throw new Error(`No se pudo cargar ${url} (HTTP ${response.status}).`);
  }
  return parseAppConfig(await response.json());
}
