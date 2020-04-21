import { LoadshopEnvironment } from '../app/shared/models/loadshop-environment';

export const environment: LoadshopEnvironment = {
  production: true,
  debug: false,
  apiUrl: 'https://tops-ws.kbxl.com/LoadShop.Web',
  identityServerUrl: 'https://identity.kbxl.com',
  siteUrl: 'https://loadshop.kbxl.com',
  baseUrl: '/',
  googleKey: 'AIzaSyD54LveBKwl1A9rWwwri1Lc3T-n4o0ixMo',
  ga_tracking_id: 'UA-131254949-2',
  tops2GoApiUrl: 'https://mobile.kbxl.com/t2g',
  recaptchaSiteKey: '6Lch5dsUAAAAAC2nrxufvhqqfG3oTEcqzvVxjLDe',
  enableUsersnap: true,
  usersnapClassicApiKey: '5a388b5e-3f4e-4c4b-9899-a531ccaa506a',
  usersnapCXApiKey: 'eb80680e-cce7-463f-a563-1f6c5b092e3d',
  usersnapFeedbackApiKey: '6eaae6d5-4007-4347-81df-8424e9d57181'
};

export const extModules = [
];
