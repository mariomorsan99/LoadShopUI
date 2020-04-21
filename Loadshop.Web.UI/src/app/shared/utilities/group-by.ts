/* eslint-disable @typescript-eslint/interface-name-prefix */

export interface IGrouping<T, TKey> {
  key: TKey;
  items: Array<T>;
}
export class Grouping<T, TKey> implements IGrouping<T, TKey> {
  key: TKey;
  items: Array<T>;

  constructor(key: TKey) {
    this.key = key;
  }
}

export function groupBy<T, TKey>(key: (item: T) => TKey, array: Array<T>) {
  return array.reduce((groups: Array<Grouping<T, TKey>>, obj: T) => {
    const value = key(obj);
    let group = groups.filter(g => g.key === value)[0];

    if (!group) {
      group = new Grouping<T, TKey>(value);
      groups.push(group);
    }

    group.items = (group.items || new Array<T>()).concat(obj);
    return groups;
  }, new Array<Grouping<T, TKey>>());
}
