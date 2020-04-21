import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { LoadshopEnvironment } from '../app/shared/models/loadshop-environment';

// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment: LoadshopEnvironment = {
  production: false,
  debug: false,
  apiUrl: 'http://localhost:5003',
  identityServerUrl: 'https://identity-t1.kbxl.com',
  siteUrl: 'http://localhost:5002',
  baseUrl: '',
  googleKey: 'AIzaSyD54LveBKwl1A9rWwwri1Lc3T-n4o0ixMo',
  ga_tracking_id: 'UA-131254949-1',
  tops2GoApiUrl: 'https://mobile-t1.kbxl.com/t2g',
  recaptchaSiteKey: '6Lds5dsUAAAAAC8Xpi-o1FOSU0ggh5EjcrpGzuVG',
  enableUsersnap: true,
  usersnapClassicApiKey: '5a388b5e-3f4e-4c4b-9899-a531ccaa506a',
  usersnapCXApiKey: 'eb80680e-cce7-463f-a563-1f6c5b092e3d',
  usersnapFeedbackApiKey: '6eaae6d5-4007-4347-81df-8424e9d57181',
};

export const extModules = [
  StoreDevtoolsModule.instrument({
    name: 'LoadShop DevTools',
    maxAge: 50,
  }),
];
