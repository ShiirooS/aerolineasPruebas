import { loadAppConfig, parseAppConfig } from './app-config';

describe('parseAppConfig', () => {
  it('acepta una configuración válida y quita la barra final de la URL', () => {
    expect(parseAppConfig({ environment: 'qa', apiBaseUrl: 'https://api.qa.example/' })).toEqual({
      environment: 'qa',
      apiBaseUrl: 'https://api.qa.example',
    });
  });

  it('rechaza un ambiente desconocido', () => {
    expect(() => parseAppConfig({ environment: 'staging', apiBaseUrl: 'http://x' })).toThrow(
      /environment/,
    );
  });

  it('rechaza una apiBaseUrl que no es URL absoluta', () => {
    expect(() => parseAppConfig({ environment: 'dev', apiBaseUrl: '/api' })).toThrow(/apiBaseUrl/);
  });

  it('rechaza algo que no es objeto', () => {
    expect(() => parseAppConfig(null)).toThrow(/objeto/);
  });
});

describe('loadAppConfig', () => {
  it('descarga y valida config.json', async () => {
    const fakeFetch = (async () =>
      new Response(JSON.stringify({ environment: 'prod', apiBaseUrl: 'https://api.example' }), {
        status: 200,
      })) as typeof fetch;

    await expect(loadAppConfig(fakeFetch)).resolves.toEqual({
      environment: 'prod',
      apiBaseUrl: 'https://api.example',
    });
  });

  it('falla con un mensaje claro si config.json no existe', async () => {
    const fakeFetch = (async () => new Response('', { status: 404 })) as typeof fetch;

    await expect(loadAppConfig(fakeFetch)).rejects.toThrow(/HTTP 404/);
  });
});
