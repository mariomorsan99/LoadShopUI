import { Params } from '@angular/router';

export function convertParamsToObject<T>(params: Params, type: { new(): T; }): T {
  const ret: T = new type();
  for (const p of Object.keys(params)) {
    ret[p] = params[p];
  }
  return ret;
}
