import { Params } from '@angular/router';

export function convertObjectToParams<T>(model: T) {
  const query: Params = {};
  for (const p of Object.keys(model)) {
    query[p] = model[p];
  }
  return query;
}
