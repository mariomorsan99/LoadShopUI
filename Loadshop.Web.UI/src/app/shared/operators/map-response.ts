import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { ServiceResponse } from '../models';

export function mapResponse<T>(): (source: Observable<ServiceResponse<T>>) => Observable<T> {
    return function (source: Observable<ServiceResponse<T>>) {
        return source.pipe(map((response: ServiceResponse<T>) => {
            if (!response.success) {
              throw new Error(response.errors.map(x => x.message).join('\n'));
            }
            return response.data;
        }));
    };
}
