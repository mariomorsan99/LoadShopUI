import { ResponseError } from './response-error';

export interface ServiceResponse<T> {
    data: T;
    errors: ResponseError[];
    success: boolean;
}
