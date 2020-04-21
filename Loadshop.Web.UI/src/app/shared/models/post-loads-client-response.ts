import { IShippingLoadDetail } from './shipping-load-detail';
import { ValidationProblemDetails } from './validation-problem-details';

export interface PostLoadsClientResponse {
  postedLoads: IShippingLoadDetail[];
  validationProblemDetails: ValidationProblemDetails;
}
