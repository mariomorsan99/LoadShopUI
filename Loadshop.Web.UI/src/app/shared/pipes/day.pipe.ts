import { Pipe, PipeTransform } from '@angular/core';
import { UserLane } from '../models';

@Pipe({ name: 'dayAbbreviation' })
export class DayAbbreviationPipe implements PipeTransform {
    transform(value: UserLane, ...args: any[]) {
        return (value.sunday ? 'Su ' : '') +
            (value.monday ? 'M ' : '') +
            (value.tuesday ? 'Tu ' : '') +
            (value.wednesday ? 'W ' : '') +
            (value.thursday ? 'Th ' : '') +
            (value.friday ? 'F ' : '') +
            (value.saturday ? 'Sa ' : '');
    }
}
