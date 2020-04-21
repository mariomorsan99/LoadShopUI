import { Environment } from '@tms-ng/core';

export interface LoadshopEnvironment extends Environment {
  googleKey: string;
  ga_tracking_id: string;
  tops2GoApiUrl: string;
  recaptchaSiteKey: string;
  enableUsersnap: boolean;
  usersnapClassicApiKey: string;
  usersnapCXApiKey: string;
  usersnapFeedbackApiKey: string;
}
