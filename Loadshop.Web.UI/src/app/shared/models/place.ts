export interface Place {
    placeType: PlaceTypeEnum;
    description: string;
    address: string;
    city: string;
    state: string;
    stateAbbrev: string;
    postalCode: string;
    country: string;
    latitude: number;
    longitude: number;
}

export enum PlaceTypeEnum {
    Address = 'ADDRESS',
    Establistment = 'ESTABLISTMENT',
    Route = 'ROUTE',
    City = 'CITY',
    State = 'STATE',
    Country = 'COUNTRY',
    Unknown = 'UNKNOWN',
    PostalCode = 'POSTAL_CODE',
}

export function setPlaceDescription(place: Place) {
    let description = '';
    if (place.placeType === PlaceTypeEnum.State) {
        description += `${place.state}, ${place.country} `;
    } else {
        if (place.address) {
            description = `${place.address}, `;
        }
        if (place.city) {
            description += `${place.city}, `;
        }
        if (place.stateAbbrev) {
            description += `${place.stateAbbrev}, `;
        }
        if (place.postalCode) {
            description += `${place.postalCode}, `;
        }
        if (place.country) {
            description += `${place.country}`;
        }
    }

    place.description = description;
}
