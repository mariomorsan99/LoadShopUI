import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, SimpleChanges, OnChanges } from '@angular/core';
import { Place, PlaceTypeEnum, setPlaceDescription } from '../../models';
import { GoogleMapService } from 'src/app/core/services';

@Component({
    selector: 'kbxl-address-autocomplete',
    templateUrl: './address-autocomplete.component.html',
    styleUrls: ['./address-autocomplete.component.scss'],
    changeDetection: ChangeDetectionStrategy.Default
})
export class AddressAutocompleteComponent implements OnChanges {
    @Input() place: Place;
    @Input() includeAddress = true;
    @Input() placeholder = 'Search by Address/City/PostalCode/State/Province';

    @Output() placeChange: EventEmitter<Place> = new EventEmitter<Place>();
    @Output() select: EventEmitter<Place> = new EventEmitter<Place>();

    public suggestions: string[];
    private innerDescription: string;

    constructor(private googleService: GoogleMapService) {}

    ngOnChanges(changes: SimpleChanges) {
        for (const key in changes) {
            if (changes.hasOwnProperty(key)) {
                if (key === 'place') {
                    this.updateValue(this.value);
                }
            }
        }
    }

    get description(): string {
        return this.innerDescription;
    }

    set description(desc: string) {
        this.innerDescription = desc;
    }

    // get accessor
    get value(): Place {
        return this.place;
    }

    // set accessor including call the onchange callback
    set value(v: Place) {
        if (v !== this.place) {
            this.updateValue(v);
            this.placeChange.emit(this.place);
        }
    }

    updateValue(v: Place) {
        this.place = v;
        this.description = v ? v.description : null;
    }

    citySearch(event: any) {
        if (this.includeAddress) {
            this.googleService.addressAutoComplete(event.query).then(x => {
                if (x && x.length === 0) {
                    this.suggestions = [];
                } else {
                    this.suggestions = x.map(y => y.description);
                }
            });
        } else {
            this.googleService.autoComplete(event.query).then(x => {
                if (x && x.length === 0) {
                    this.suggestions = [];
                } else {
                    this.suggestions = x.map(y => y.description);
                }
            });
        }
    }

    clearValue() {
        this.value = null;
    }

    populateValue(description: string) {
        this.populatePlace(description).then(x => {
            this.value = x;
            this.select.emit(x);
        });
    }

    validateSelection(event: any) {
        if (event.target && this.value && event.target.value !== this.value.description && event.target.value !== '') {
            this.description = this.value.description;
        }
    }

    private populatePlace(description: string): Promise<Place> {
        return this.googleService.geocodePlace(description).then(x => {
            if (x.placeType === PlaceTypeEnum.Establistment) {
                x.placeType = PlaceTypeEnum.Address;
            }
            if (x.placeType === PlaceTypeEnum.Route) {
                if (x.address) {
                    x.placeType = PlaceTypeEnum.Address;
                } else {
                    x.address = null;
                    x.placeType = PlaceTypeEnum.City;
                }
            }

            setPlaceDescription(x);
            return x;
        });
    }
}
