import { Component, ElementRef, ViewChild, AfterViewInit, Input, OnChanges, Output, EventEmitter } from '@angular/core';
import { GoogleMapService } from '../../../core/services/google-map.service';
import { Directions } from '../../models/directions';
import { LoadStop } from '../../models';

@Component({
  selector: 'kbxl-map',
  template: '<div #map style="height: 100%"></div>'
})
export class MapComponent implements AfterViewInit, OnChanges {
  map: google.maps.Map;
  @ViewChild('map') mapPlaceHolder;
  @Input() stops: LoadStop[];
  @Output() directions: EventEmitter<Directions> = new EventEmitter<Directions>();

  constructor(public element: ElementRef, private googleService: GoogleMapService) { }

  ngAfterViewInit() {
    this.googleService.createMap(this.mapPlaceHolder).then(x => {
      x.setOptions({ maxZoom: 10, streetViewControl: false });
      this.map = x;
      this.googleService.setDrawingMap(this.map);
    });
  }

  ngOnChanges() {
    if (this.stops && this.stops.length > 1) {
      this.googleService.getDirections(this.stops).then(x => {
        if (x && x.routes && x.routes.length) {
          this.googleService.drawDirections(x).then(y => {
            const route = x.routes[0];
            if (route.legs) {
              const directions = route.legs
                .map(leg => new Directions(leg.distance.value, leg.duration.value))
                .reduce((acc, leg) => {
                  return new Directions(acc.distance + leg.distance, acc.distance + leg.duration);
                });
              this.directions.emit(directions);
            }
          });
        }
      });
    }
  }
}
