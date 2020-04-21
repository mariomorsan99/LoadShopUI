import { Pipe, PipeTransform } from '@angular/core';
import { SelectItem } from 'primeng/components/common/selectitem';

@Pipe({ name: 'toSelectItemByKey' })
export class ToSelectItemByKeyPipe implements PipeTransform {
  public transform(list: any[], labelKey: string, valueKey: string): SelectItem[] {
    return list.map(item => ({ label: this.buildLabel(item, labelKey), value: item[valueKey] }));
  }

  private buildLabel(item: any, labelKey: string): string {
    const keys = labelKey.split(',');
    let label = '';

    keys.forEach((key, index) => {

      if (index > 0 && index + 1 < key.length) {
        label += ' ';
      }
      label += item[key];

    });

    return label;
  }
}
