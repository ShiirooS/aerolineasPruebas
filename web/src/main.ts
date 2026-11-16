import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { APP_CONFIG, loadAppConfig } from './app/core/config/app-config';

// La configuración del ambiente se lee antes de arrancar Angular, así cualquier
// servicio puede inyectar APP_CONFIG de forma síncrona.
loadAppConfig()
  .then((config) =>
    bootstrapApplication(App, {
      ...appConfig,
      providers: [...appConfig.providers, { provide: APP_CONFIG, useValue: config }],
    }),
  )
  .catch((err) => console.error(err));
