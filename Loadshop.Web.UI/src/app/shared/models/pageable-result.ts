export interface PageableResult<T> {
  data: T[];
  totalRecords: number;
}

export class DefaultPageableResult<T> implements PageableResult<T> {
  data: T[] = [];
  totalRecords: 0;

  static Create<T>(): PageableResult<T> {
    return new DefaultPageableResult();
  }
}
