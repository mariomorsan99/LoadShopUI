import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { ConfirmationService, Message, TreeNode } from 'primeng/api';
import { GoogleMapService } from '../../../core/services';
import { GeoLocation, Place, State, UserLane } from '../../../shared/models';
import { deepCopyLane, groupBy, validateUserlane } from '../../../shared/utilities';

@Component({
  selector: 'kbxl-user-lane-detail',
  templateUrl: './user-lane-detail.component.html',
  styleUrls: ['./user-lane-detail.component.scss'],
})
export class UserLaneDetailComponent implements OnChanges {
  @Input() lane: UserLane;
  @Input() states: State[];
  @Input() equipment: TreeNode[];
  @Output() updated = new EventEmitter<UserLane>();
  @Output() deleted = new EventEmitter<UserLane>();
  @Output() closed = new EventEmitter<boolean>();
  errors: Message[];
  origin: Place;
  dest: Place;
  adding = true;
  selectedEquipment: TreeNode[];

  constructor(private googleService: GoogleMapService, private confirmationService: ConfirmationService) {}

  ngOnChanges() {
    this.dest = null;
    this.origin = null;
    this.adding = !this.lane || !this.lane.userLaneId;

    if (this.lane && this.states) {
      this.googleService.getOrigin(this.lane, this.states).then(x => {
        this.origin = x;
      });

      this.googleService.getDestination(this.lane, this.states).then(x => {
        this.dest = x;
      });
    }

    if (this.lane && this.equipment) {
      this.updateSelectedEquipment();
    }
  }

  update() {
    const lane = deepCopyLane(this.lane);
    lane.equipmentIds = this.buildSelectedEquipment();
    this.setOriginDestData(lane).then(l => {
      this.errors = validateUserlane(l);
      if (!this.errors.length) {
        this.updated.emit(l);
        this.clear();
      }
    });
  }

  delete() {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete this favorite lane?`,
      accept: () => {
        this.deleted.emit(this.lane);
        this.clear();
      },
    });
  }

  close() {
    this.closed.emit(true);
    this.clear();
  }

  geocode(location: Place): Promise<GeoLocation> {
    if (location && location.description && location.city) {
      return this.googleService.geocode(location.description);
    }
    return new Promise(resolve => resolve(null));
  }

  setOriginDestData(lane: UserLane) {
    let origin: Place = null;
    let dest: Place = null;
    lane.origCity = null;
    lane.origCountry = null;
    lane.origState = null;
    lane.destCity = null;
    lane.destCountry = null;
    lane.destState = null;
    if (this.origin) {
      origin = this.origin;
    }
    if (this.dest) {
      dest = this.dest;
    }
    return Promise.all([this.geocode(origin), this.geocode(dest)]).then(results => {
      if (this.origin) {
        lane.origCity = this.origin.city;
        if (!this.origin.stateAbbrev && this.origin.state && this.origin.state.length > 2) {
          lane.origState = this.states.find(x => x.name.toLowerCase() === this.origin.state.toLowerCase()).abbreviation;
        } else {
          lane.origState = this.origin.stateAbbrev;
        }
        lane.origCountry = this.origin.country;
        if (results[0]) {
          lane.origLat = results[0].latitude;
          lane.origLng = results[0].longitude;
        } else {
          lane.origDH = null;
          lane.origLat = null;
          lane.origLng = null;
        }
      }
      if (this.dest) {
        lane.destCity = this.dest.city;
        if (!this.dest.stateAbbrev && this.dest.state && this.dest.state.length > 2) {
          lane.destState = this.states.find(x => x.name.toLowerCase() === this.dest.state.toLowerCase()).abbreviation;
        } else {
          lane.destState = this.dest.stateAbbrev;
        }
        lane.destCountry = this.dest.country;

        if (results[1]) {
          lane.destLat = results[1].latitude;
          lane.destLng = results[1].longitude;
        } else {
          lane.destDH = null;
          lane.destLat = null;
          lane.destLng = null;
        }
      }
      return lane;
    });
  }

  private updateSelectedEquipment() {
    if (this.lane && this.lane.equipmentIds && this.equipment) {
      const flattenTreeNodes = this.equipment.map(_ => _.children).reduce((acc, value) => acc.concat(value));

      const equipmentTreeNodes = this.lane.equipmentIds.map(currentEquipmentId => {
        const equipment = flattenTreeNodes.find(_ => _.data.equipmentId === currentEquipmentId);

        if (equipment && equipment.parent) {
          equipment.parent.partialSelected = true;
        }

        return equipment;
      });

      const groupedSelections = groupBy(x => x.data.categoryId, equipmentTreeNodes);

      groupedSelections.forEach(group => {
        const treeViewGroup = this.equipment.find(x => x.key === group.key);

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
    return this.selectedEquipment.filter(_ => _.leaf).map(_ => _.data.equipmentId);
  }

  clear() {
    this.origin = null;
    this.dest = null;
    this.errors = [];
  }
}
