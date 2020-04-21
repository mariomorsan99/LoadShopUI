export interface PageableQueryData {
  pageSize: number;
  pageNumber: number;
  filter: unknown;
  sortField: string;
  descending: boolean;
}

export class PageableQueryHelper implements PageableQueryData {
  constructor(
    public pageSize: number,
    public pageNumber: number,
    public filter: unknown = null,
    public sortField: string = null,
    public descending: boolean = null
  ) {}

  static default(): PageableQueryHelper {
    return new PageableQueryHelper(50, 1, null);
  }

  generateQuery(): string {
    const skip = (this.pageNumber - 1) * this.pageSize;

    const query = {
      skip,
      take: this.pageSize,
      filter: this.filter,
      descending: this.descending,
      orderby: this.capitlizeFirstLetter(this.sortField),
    };

    return (
      '?' +
      Object.keys(query)
        .filter(key => key !== 'filter' && query[key])
        .map(key => key + '=' + query[key])
        .join('&')
    );
  }

  capitlizeFirstLetter(str: string): string {
    if (str) {
      return str.replace(/^\w/, c => c.toUpperCase());
    }
    return null;
  }
}
