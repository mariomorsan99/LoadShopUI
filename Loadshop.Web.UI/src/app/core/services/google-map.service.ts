import { ElementRef, Injectable } from '@angular/core';
import * as countries from 'i18n-iso-countries';
import { ReplaySubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { GeoLocation, LoadStop, Place, PlaceTypeEnum, setPlaceDescription, State, UserLane } from '../../shared/models';
import { createPlace } from '../../shared/utilities/create-place';

declare let google;

@Injectable({ providedIn: 'root' })
export class GoogleMapService {
  private geocoder: google.maps.Geocoder;
  private autoCompleteService: google.maps.places.AutocompleteService;
  private directionsService: google.maps.DirectionsService;
  private directionsRenderer: google.maps.DirectionsRenderer;
  private loaded: ReplaySubject<boolean> = new ReplaySubject<boolean>();
  private verifyLoaded = this.loaded.pipe(filter(x => !!x));
  private radius = 3956;
  private pidiv180 = 0.017453293;

  private gMap: google.maps.Map;
  initialize() {
    this.geocoder = new google.maps.Geocoder();
    this.autoCompleteService = new google.maps.places.AutocompleteService();
    this.directionsService = new google.maps.DirectionsService();
    this.directionsRenderer = new google.maps.DirectionsRenderer();
    this.loaded.next(true);
    this.loaded.complete();
  }

  getOrigin(model: UserLane, states: State[]): Promise<Place> {
    return new Promise(resolve => {
      if (model) {
        if (model.origLat && model.origLng) {
          if (model.origCity) {
            resolve(createPlace(null, model.origCity, model.origState, null, null, model.origCountry));
          } else {
            this.reverseGeocode(model.origLat, model.origLng).then(x => {
              resolve(x);
            });
          }
        } else if (model.origState) {
          const state = states.find(x => x.abbreviation.toLowerCase() === model.origState.toLowerCase());
          if (state) {
            resolve(createPlace(null, null, state.name, model.origState, null, model.origCountry));
          }
        }
      } else {
        resolve(null);
      }
    });
  }

  getDestination(model: UserLane, states: State[]): Promise<Place> {
    return new Promise(resolve => {
      if (model) {
        if (model.destLat && model.destLng) {
          let destState = model.destState;
          if (model.destState && model.destState.length === 2) {
            const destStateObj = states.find(x => x.abbreviation.toLowerCase() === model.destState.toLowerCase());
            destState = destStateObj.name;
          }

          if (model.destCity) {
            resolve(createPlace(null, model.destCity, destState, model.destState, null, model.destCountry));
          } else {
            this.reverseGeocode(model.destLat, model.destLng).then(x => {
              resolve(x);
            });
          }
        } else if (model.destState) {
          const state = states.find(x => x.abbreviation.toLowerCase() === model.destState.toLowerCase());
          if (state) {
            resolve(createPlace(null, null, state.name, model.destState, null, model.destCountry));
          }
        }
      } else {
        resolve(null);
      }
    });
  }

  geocode(address: string): Promise<GeoLocation> {
    return new Promise(resolve => {
      if (!address) {
        resolve(null);
      } else {
        return this.verifyLoaded.toPromise().then(() => {
          this.geocoder.geocode({ address: address }, results => {
            resolve(convertToGeoLocation(results[0]));
          });
        });
      }
    });
  }

  geocodePlace(address: string): Promise<Place> {
    return new Promise(resolve => {
      if (!address) {
        resolve(null);
      } else {
        return this.verifyLoaded.toPromise().then(() => {
          this.geocoder.geocode({ address: address }, results => {
            resolve(convertToPlace(results));
          });
        });
      }
    });
  }

  reverseGeocode(lat: number, lng: number): Promise<Place> {
    return new Promise(resolve => {
      if (!lat || !lng) {
        resolve(null);
      } else {
        return this.verifyLoaded.toPromise().then(() => {
          this.geocoder.geocode(
            {
              location: {
                lat: parseFloat(lat.toString()),
                lng: parseFloat(lng.toString()),
              },
            },
            results => {
              resolve(convertToPlace(results));
            }
          );
        });
      }
    });
  }

  autoComplete(input: string): Promise<Place[]> {
    return new Promise(resolve => {
      return this.verifyLoaded.toPromise().then(() => {
        this.autoCompleteService.getPlacePredictions(
          {
            input: input,
            types: ['(regions)'],
            componentRestrictions: {
              country: ['us', 'ca'],
            },
          },
          results => {
            resolve(convertToPlaceArray(results));
          }
        );
      });
    });
  }

  addressAutoComplete(input: string): Promise<Place[]> {
    return new Promise(resolve => {
      return this.verifyLoaded.toPromise().then(() => {
        this.autoCompleteService.getPlacePredictions(
          {
            input: input,
            componentRestrictions: {
              country: ['us', 'ca'],
            },
          },
          results => {
            resolve(convertToPlaceArray(results));
          }
        );
      });
    });
  }

  createMap(element: ElementRef): Promise<google.maps.Map> {
    return new Promise(resolve => {
      return this.verifyLoaded.toPromise().then(() => {
        resolve(
          new google.maps.Map(element.nativeElement, {
            center: { lat: 39.962912, lng: -96.108648 },
            zoom: 4,
            maxZoom: 15,
            mapTypeControl: false,
          })
        );
      });
    });
  }

  getDirections(stops: LoadStop[]): Promise<google.maps.DirectionsResult> {
    return new Promise(resolve => {
      const s = stops.slice(1, stops.length - 1);
      const points: google.maps.DirectionsWaypoint[] = s.map(x => {
        return { location: convertStopToLatLng(x), stopover: true };
      });
      return this.verifyLoaded.toPromise().then(() => {
        this.directionsService.route(
          {
            origin: convertStopToLatLng(stops[0]),
            destination: convertStopToLatLng(stops[stops.length - 1]),
            waypoints: points,
            travelMode: google.maps.TravelMode.DRIVING,
          },
          result => {
            resolve(result);
          }
        );
      });
    });
  }

  setDrawingMap(map: google.maps.Map): Promise<boolean> {
    return new Promise(resolve => {
      return this.verifyLoaded.toPromise().then(() => {
        // save a copy of the map to reinit rendered as this will get wiped
        this.gMap = map;
        this.directionsRenderer.setMap(map);
        resolve(true);
      });
    });
  }

  drawDirections(directions: google.maps.DirectionsResult): Promise<boolean> {
    return new Promise(resolve => {
      return this.verifyLoaded.toPromise().then(() => {
        for (let i = 0; i < directions.routes.length; i++) {
          const route = directions.routes[i];
          for (let j = 0; j < route.legs.length; j++) {
            const leg = route.legs[j];
            const l = route.legs.length;
            if (j === 0) {
              leg.start_address = 'Origin';
              if (l === 1) {
                leg.end_address = 'Destination';
              } else {
                leg.end_address = 'Stop';
              }
            } else if (j === l - 1) {
              leg.end_address = 'Destination';
              if (l === 1) {
                leg.start_address = 'Origin';
              } else {
                leg.start_address = 'Stop';
              }
            } else {
              leg.start_address = 'Stop';
              leg.end_address = 'Stop';
            }
          }
        }
        // reinit the rendered to ensure we have the same map
        this.directionsRenderer.setMap(this.gMap);
        this.directionsRenderer.setDirections(directions);
        resolve(true);
      });
    });
  }

  calculateDistance(lat1: number, lng1: number, lat2: number, lng2: number): number {
    const radlat1 = lat1 * this.pidiv180;
    const radlng1 = lng1 * this.pidiv180;
    const radlat2 = lat2 * this.pidiv180;
    const radlng2 = lng2 * this.pidiv180;
    const dLat = radlat2 - radlat1;
    const dLng = radlng2 - radlng1;
    const a = Math.pow(Math.sin(dLat / 2), 2) + Math.cos(radlat1) * Math.cos(radlat2) * Math.pow(Math.sin(dLng / 2), 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const d = this.radius * c;
    return Math.round(d);
  }
}

function convertToGeoLocation(data: google.maps.GeocoderResult): GeoLocation {
  return {
    address: data.formatted_address,
    latitude: data.geometry.location.lat(),
    longitude: data.geometry.location.lng(),
  };
}

function convertToPlace(data: google.maps.GeocoderResult[]): Place {
  if (!data || !data.length) {
    return null;
  }
  const p = data[0];
  const street_number = p.address_components.find(x => !!x.types.filter(y => y === 'street_number').length);
  const route = p.address_components.find(x => !!x.types.filter(y => y === 'route').length);
  const city = p.address_components.find(x => !!x.types.filter(y => y === 'locality').length);
  const state = p.address_components.find(x => !!x.types.filter(y => y === 'administrative_area_level_1').length);
  const country = p.address_components.find(x => !!x.types.filter(y => y === 'country').length);
  const postalCode = p.address_components.find(x => !!x.types.filter(y => y === 'postal_code').length);
  const location = p.geometry.location;
  const place: Place = {
    placeType: determinePlaceType(p.types),
    description: null,
    address: street_number ? `${street_number.long_name} ${route ? ` ${route.long_name}` : ''}` : null,
    city: city ? city.long_name : null,
    state: state ? state.long_name : null,
    stateAbbrev: state ? state.short_name : null,
    postalCode: postalCode ? postalCode.long_name : null,
    country: countries.alpha2ToAlpha3(country.short_name),
    latitude: location.lat(),
    longitude: location.lng(),
  };

  setPlaceDescription(place);
  return place;
}

function convertToPlaceArray(data: google.maps.places.AutocompletePrediction[]): Place[] {
  return !data
    ? []
    : data.map(x => {
        // Country
        if (x.types[0] === 'country') {
          return {
            placeType: PlaceTypeEnum.Country,
            description: x.description,
            address: null,
            city: null,
            state: null,
            stateAbbrev: null,
            postalCode: null,
            latitude: null,
            longitude: null,
            country: x.terms[0] ? x.terms[0].value : null,
          };
        }
        if (x.types.find(y => y === 'administrative_area_level_1')) {
          // State, Country
          if (x.terms.length === 2) {
            return {
              placeType: PlaceTypeEnum.State,
              description: x.description,
              address: null,
              city: null,
              state: x.terms[0] ? x.terms[0].value : null,
              stateAbbrev: null,
              postalCode: null,
              country: x.terms[1] ? x.terms[1].value : null,
              latitude: null,
              longitude: null,
            };
          }
        }

        if (x.types.find(y => y === 'administrative_area_level_2')) {
          // State, Country
          if (x.terms.length === 3) {
            return {
              placeType: PlaceTypeEnum.State,
              description: x.description,
              address: null,
              city: null,
              state: null,
              stateAbbrev: x.terms[1] ? x.terms[1].value : null,
              postalCode: null,
              country: x.terms[2] ? x.terms[2].value : null,
              latitude: null,
              longitude: null,
            };
          }
        }

        const terms = x.terms.length - 1;
        const containsZip = x.terms[terms - 1].value.length !== 2;
        let place: Place;

        if (containsZip) {
          place = {
            placeType: PlaceTypeEnum.Unknown,
            description: x.description,
            address: null,
            city: x.terms[terms - 3] ? x.terms[terms - 3].value : null,
            state: null,
            stateAbbrev: x.terms[terms - 2] ? x.terms[terms - 2].value : null,
            postalCode: x.terms[terms - 1] ? x.terms[terms - 1].value : null,
            country: x.terms[terms] ? x.terms[terms].value : null,
            latitude: null,
            longitude: null,
          };
        } else {
          place = {
            placeType: PlaceTypeEnum.Unknown,
            description: x.description,
            address: null,
            city: x.terms[terms - 2] ? x.terms[terms - 2].value : null,
            state: null,
            stateAbbrev: x.terms[terms - 1] ? x.terms[terms - 1].value : null,
            postalCode: null,
            country: x.terms[terms] ? x.terms[terms].value : null,
            latitude: null,
            longitude: null,
          };
        }

        if (x.types.find(y => y === 'postal_code')) {
          place.placeType = PlaceTypeEnum.PostalCode;
        }

        if (x.types.find(y => y === 'locality')) {
          // City, State, Country
          place.placeType = PlaceTypeEnum.City;
        }

        if (x.types.find(y => y === 'establishment' || y === 'neighborhood')) {
          place.placeType = PlaceTypeEnum.Establistment;
        }

        if (x.types.find(y => y === 'route')) {
          place.placeType = PlaceTypeEnum.Route;
          place.address = x.structured_formatting.main_text;
        }

        if (x.types.find(y => y === 'street_address' || y === 'premise')) {
          // Address, City, State, (Zip,) Country
          // Address can be ['123', "blah street"] or ['123 blah street']
          place.placeType = PlaceTypeEnum.Address;
          place.address = x.structured_formatting.main_text;
        }

        return place;
      });
}

function determinePlaceType(types: string[]): PlaceTypeEnum {
  if (types[0] === 'country') {
    return PlaceTypeEnum.Country;
  }
  if (types.find(y => y === 'administrative_area_level_1')) {
    return PlaceTypeEnum.State;
  }

  if (types.find(y => y === 'administrative_area_level_2')) {
    return PlaceTypeEnum.State;
  }

  if (types.find(y => y === 'postal_code')) {
    return PlaceTypeEnum.PostalCode;
  }

  if (types.find(y => y === 'locality')) {
    return PlaceTypeEnum.City;
  }

  if (types.find(y => y === 'establishment' || y === 'neighborhood')) {
    return PlaceTypeEnum.Establistment;
  }

  if (types.find(y => y === 'route')) {
    return PlaceTypeEnum.Route;
  }

  if (types.find(y => y === 'street_address' || y === 'premise')) {
    return PlaceTypeEnum.Address;
  }

  return PlaceTypeEnum.Unknown;
}

function convertStopToLatLng(stop: LoadStop) {
  return `${stop.latitude},${stop.longitude}`;
}
