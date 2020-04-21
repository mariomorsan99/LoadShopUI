import { StoreDevtoolsModule } from '@ngrx/store-devtools';
import { LoadshopEnvironment } from '../app/shared/models/loadshop-environment';
export const environment: LoadshopEnvironment = {
  production: false,
  debug: false,
  apiUrl: 'https://tops-t3.kbxl.com/loadshopapi',
  identityServerUrl: 'https://identity-t3.kbxl.com',
  siteUrl: 'https://tops-t3.kbxl.com/loadshop',
  baseUrl: 'loadshop',
  googleKey: 'AIzaSyD54LveBKwl1A9rWwwri1Lc3T-n4o0ixMo',
  ga_tracking_id: 'UA-131254949-1',
  tops2GoApiUrl: 'https://mobile-t3.kbxl.com/t2g',
  recaptchaSiteKey: '6Lds5dsUAAAAAC8Xpi-o1FOSU0ggh5EjcrpGzuVG',
  enableUsersnap: false,
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
