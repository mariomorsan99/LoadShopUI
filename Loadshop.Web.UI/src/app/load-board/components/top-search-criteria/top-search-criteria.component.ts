import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import * as moment from 'moment';
import { MessageService, SelectItem, TreeNode } from 'primeng/api';
import { GoogleMapService } from '../../../core/services';
import {
  defaultLane,
  defaultSearch,
  GeoLocation,
  Place,
  Search,
  SearchTypeData,
  ServiceType,
  State,
  UserLane,
} from '../../../shared/models';
import { formatDate, groupBy, validateUserlane } from '../../../shared/utilities';
import { createPlace } from '../../../shared/utilities/create-place';

@Component({
  selector: 'kbxl-top-search-criteria',
  templateUrl: './top-search-criteria.component.html',
  styleUrls: ['./top-search-criteria.component.scss'],
})
export class TopSearchCriteriaComponent implements OnChanges {
  @Input() equipment: TreeNode[];
  @Input() criteria: Search;
  @Input() lanes: UserLane[];
  @Input() states: State[];
  @Input() isMobile: boolean;
  @Input() isMarketplace: boolean;
  @Input() serviceTypes: ServiceType[];
  @Output() search: EventEmitter<Search> = new EventEmitter<Search>();
  @Output() add: EventEmitter<UserLane> = new EventEmitter<UserLane>();
  @Output() update: EventEmitter<UserLane> = new EventEmitter<UserLane>();
  @Output() clear: EventEmitter<string> = new EventEmitter<string>();

  crit: Search = { ...defaultSearch };
  origDate: Date[];
  destDate: Date[];
  selectedLane: UserLane;
  selectedLaneId: string;
  selectLanes: SelectItem[];
  origin: Place;
  dest: Place;
  userLaneId: string;
  collapsed = true;
  selectedEquipment: TreeNode[];
  quickSearch: string;
  selectedServiceTypes: ServiceType[];

  constructor(private googleService: GoogleMapService, private messageService: MessageService) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes.lanes && this.lanes) {
      this.selectLanes = this.lanes.map((x) => {
        return { label: this.createLaneLabel(x), value: x.userLaneId };
      });
    }
    if (changes.criteria) {
      const crit = this.criteria || { ...defaultSearch };
      this.crit = { ...crit };
      this.origin = null;
      this.dest = null;

      this.updateSelectedEquipment();
      this.setDates(this.crit);
      this.setOriginDestination(this.crit);
    }
    if (changes.states) {
      this.setOriginDestination(this.crit);
    }
  }

  setDates(model: Search) {
    if (!model) {
      this.origDate = null;
      this.destDate = null;
      return;
    }
    if (model.origDateStart) {
      if (model.origDateEnd) {
        this.origDate = [moment(model.origDateStart).toDate(), moment(model.origDateEnd).toDate()];
      } else {
        this.origDate = [moment(model.origDateStart).toDate()];
      }
    } else {
      this.origDate = null;
    }

    if (model.destDateStart) {
      if (model.destDateEnd) {
        this.destDate = [moment(model.destDateStart).toDate(), moment(model.destDateEnd).toDate()];
      } else {
        this.destDate = [moment(model.destDateStart).toDate()];
      }
    } else {
      this.destDate = null;
    }
  }

  setOriginDestination(model: UserLane | Search) {
    if (model.origLat && model.origLng) {
      if (model.origCity) {
        this.origin = createPlace(null, model.origCity, null, model.origState, null, model.origCountry);
      } else {
        this.googleService.reverseGeocode(model.origLat, model.origLng).then((x) => {
          this.origin = x;
        });
      }
    } else if (model.origState) {
      const state = this.states.find((x) => x.abbreviation.toLowerCase() === model.origState.toLowerCase());
      if (state) {
        this.origin = createPlace(null, null, state.name, model.origState, null, model.origCountry);
      }
    }
    if (model.destLat && model.destLng) {
      if (model.destCity) {
        this.dest = createPlace(null, model.destCity, null, model.destState, null, model.destCountry);
      } else {
        this.googleService.reverseGeocode(model.destLat, model.destLng).then((x) => {
          this.dest = x;
        });
      }
    } else if (model.destState) {
      const state = this.states.find((x) => x.abbreviation.toLowerCase() === model.destState.toLowerCase());
      if (state) {
        this.dest = createPlace(null, null, state.name, model.destState, null, model.destCountry);
      }
    }
  }

  runSearch(applying = false) {
    // skip the search if we are in mobile mode and we are not applying from a button
    if ((this.isMobile && !applying) || !this.isMarketplace) {
      return;
    }

    this.setSearchData().then((search) => {
      this.search.emit(search);
      this.collapsed = true;
    });
  }

  searchClick() {
    this.setSearchData().then((search) => {
      this.search.emit(search);
      this.collapsed = true;
    });
  }

  setSearchData(): Promise<Search> {
    const origDate = this.origDate || [];
    const destDate = this.destDate || [];
    const search: Search = {
      origDateStart: formatDate(origDate[0]),
      origDateEnd: formatDate(origDate[1]),
      destDateStart: formatDate(destDate[0]),
      destDateEnd: formatDate(destDate[1]),
      equipmentType: this.buildSelectedEquipmentAsString(),
      quickSearch: this.quickSearch,
    };

    if (this.selectedServiceTypes) {
      search.serviceTypes = this.selectedServiceTypes.map((x) => x.serviceTypeId);
    }

    return Promise.all([this.geocode(this.origin), this.geocode(this.dest)]).then((results) => {
      if (this.origin) {
        search.origCity = this.origin.city;
        if (!this.origin.stateAbbrev && this.origin.state && this.origin.state.length > 2) {
          search.origState = this.states.find((x) => x.name.toLowerCase() === this.origin.state.toLowerCase()).abbreviation;
        } else {
          search.origState = this.origin.stateAbbrev;
        }
        search.origCountry = this.origin.country;
        if (results[0]) {
          search.origLat = results[0].latitude;
          search.origLng = results[0].longitude;
        }
      }
      search.origDH = this.crit.origDH || 50;
      if (this.dest) {
        search.destCity = this.dest.city;
        if (!this.dest.stateAbbrev && this.dest.state && this.dest.state.length > 2) {
          search.destState = this.states.find((x) => x.name.toLowerCase() === this.dest.state.toLowerCase()).abbreviation;
        } else {
          search.destState = this.dest.stateAbbrev;
        }
        search.destCountry = this.dest.country;
        if (results[1]) {
          search.destLat = results[1].latitude;
          search.destLng = results[1].longitude;
        }
      }
      search.destDH = this.crit.destDH || 50;
      return search;
    });
  }

  geocode(location: Place): Promise<GeoLocation> {
    if (location && location.description && location.city) {
      return this.googleService.geocode(location.description);
    }
    return new Promise((resolve) => resolve(null));
  }

  loadFavorite(laneId: string) {
    this.crit = { ...defaultSearch };
    this.userLaneId = laneId;
    this.selectedLane = this.lanes.find((x) => x.userLaneId === laneId);
    if (this.selectedLane) {
      this.origin = null;
      this.dest = null;
      this.setOriginDestination(this.selectedLane);
      this.crit.origDH = this.selectedLane.origDH;
      this.crit.destDH = this.selectedLane.destDH;
      this.crit.equipmentType = JSON.stringify(this.selectedLane.equipmentIds);
      this.setDates(null);
      // Set Selected Equipment Tree Nodes
      this.updateSelectedEquipment();
    }
    this.runSearch(true);
  }

  clearClick($event) {
    this.userLaneId = null;
    this.selectedLaneId = null;
    this.clear.emit();
    this.collapsed = true;
    $event.stopPropagation();
  }

  clearFilter(prop: any) {
    this[prop] = null;
    this.runSearch();
  }

  saveClick() {
    this.setSearchData().then((search) => {
      const lane: UserLane = Object.assign({}, defaultLane, {
        equipmentIds: this.buildSelectedEquipment(),
        origLat: search.origLat,
        origLng: search.origLng,
        origDH: search.origDH,
        origState: search.origState,
        destLat: search.destLat,
        destLng: search.destLng,
        destDH: search.destDH,
        destState: search.destState,
        searchType: SearchTypeData.OriginDest,
      });
      if (this.origin) {
        lane.origCity = this.origin.city;
        lane.origCountry = this.origin.country;
      }
      if (this.dest) {
        lane.destCity = this.dest.city;
        lane.destCountry = this.dest.country;
      }
      const errors = validateUserlane(lane);
      if (errors.length) {
        this.messageService.addAll(errors);
      } else {
        if (this.userLaneId) {
          lane.userLaneId = this.userLaneId;
          this.update.emit(lane);
        } else {
          this.add.emit(lane);
        }
      }
    });
  }

  applyClick($event: Event) {
    this.runSearch(true);
    $event.stopPropagation();
  }

  private updateSelectedEquipment() {
    if (this.crit && this.crit.equipmentType && this.equipment) {
      const flattenTreeNodes = this.equipment.map((_) => _.children).reduce((acc, value) => acc.concat(value));
      const equipmentTypes = JSON.parse(this.crit.equipmentType) as string[];
      const equipmentTreeNodes = equipmentTypes.map((currentEquipmentId) => {
        const equipment = flattenTreeNodes.find((_) => _.data.equipmentId === currentEquipmentId);

        if (equipment && equipment.parent) {
          equipment.parent.partialSelected = true;
        }

        return equipment;
      });

      const groupedSelections = groupBy((x) => x.data.categoryId, equipmentTreeNodes);

      groupedSelections.forEach((group) => {
        const treeViewGroup = this.equipment.find((x) => x.key === group.key);

        if (group.items.length === treeViewGroup.children.length) {
          treeViewGroup.partialSelected = false;
          equipmentTreeNodes.push(treeViewGroup);
        }
      });

      this.selectedEquipment = equipmentTreeNodes;
    } else {
      this.selectedEquipment = [];
    }
  }

  private buildSelectedEquipment() {
    return this.selectedEquipment.filter((_) => _.leaf).map((_) => _.data.equipmentId);
  }

  private buildSelectedEquipmentAsString() {
    const selectedEquipment = this.buildSelectedEquipment();
    if (selectedEquipment.length > 0) {
      return JSON.stringify(this.buildSelectedEquipment());
    }

    return null;
  }

  private createLaneLabel(lane: UserLane): string {
    let orig = '<div>';
    if (lane.origCity) {
      orig += `<span>${lane.origCity}, ${lane.origState}`;
      if (lane.origDH) {
        orig += ` (${lane.origDH} mi)`;
      }
      orig += '</span>';
    }
    if (!lane.origCity && lane.origState) {
      orig = `<span>${lane.origState}</span>`;
    }
    if (!lane.origCity && !lane.origState) {
      orig += '<span>Anywhere</span>';
    }
    orig += '</div>';

    let dest = '<div>';
    if (lane.destCity) {
      dest += `<span>${lane.destCity}, ${lane.destState}`;
      if (lane.origDH) {
        dest += ` (${lane.destDH} mi)`;
      }
      dest += '</span>';
    }
    if (!lane.destCity && lane.destState) {
      dest += `<span>${lane.destState}</span>`;
    }
    if (!lane.destCity && !lane.destState) {
      dest += '<span>Anywhere</span>';
    }
    dest += '</div>';
    return orig + '<div>&darr;</div>' + dest;
  }
}
