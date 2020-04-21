import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { UserLane } from '../../models';

@Component({
    selector: 'kbxl-days',
    templateUrl: './days.component.html',
    styleUrls: ['./days.component.css']
})
export class DaysComponent implements OnChanges {
    possibleDays = [
        { label: 'S', value: 'Sunday' },
        { label: 'M', value: 'Monday' },
        { label: 'T', value: 'Tuesday' },
        { label: 'W', value: 'Wednesday' },
        { label: 'T', value: 'Thursday' },
        { label: 'F', value: 'Friday' },
        { label: 'S', value: 'Saturday' }
      ];
    days: string[];

    @Input() lane: UserLane;
    @Output() laneChange: EventEmitter<UserLane> = new EventEmitter<UserLane>();

    ngOnChanges() {
        const lane = this.lane;
        this.days = this.possibleDays
            .filter(x => !!lane[x.value.toLowerCase()])
            .map(x => x.value);
    }

    onChange() {
        this.lane.sunday = !!this.days.find(x => x === 'Sunday');
        this.lane.monday = !!this.days.find(x => x === 'Monday');
        this.lane.tuesday = !!this.days.find(x => x === 'Tuesday');
        this.lane.wednesday = !!this.days.find(x => x === 'Wednesday');
        this.lane.thursday = !!this.days.find(x => x === 'Thursday');
        this.lane.friday = !!this.days.find(x => x === 'Friday');
        this.lane.saturday = !!this.days.find(x => x === 'Saturday');
        this.laneChange.emit(this.lane);
    }
}
