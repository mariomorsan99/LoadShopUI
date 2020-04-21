import { Place, PlaceTypeEnum, setPlaceDescription } from '../models';

export function createPlace(
  address: string,
  city: string,
  state: string,
  stateAbbrev: string,
  postalCode: string,
  country: string,
): Place {
  const place: Place = {
    placeType: PlaceTypeEnum.Country,
    address: address,
    city: city,
    state: state,
    stateAbbrev: stateAbbrev,
    postalCode: postalCode,
    country: country,
    latitude: null,
    longitude: null,
    description: null
  };

  if (address) {
    place.placeType = PlaceTypeEnum.Address;
  } else if (city) {
    place.placeType = PlaceTypeEnum.City;
  } else if (state) {
    place.placeType = PlaceTypeEnum.State;
  }

  setPlaceDescription(place);

  place.placeType = PlaceTypeEnum.City;
  return {...place};
}
