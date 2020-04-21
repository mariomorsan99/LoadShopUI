import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { SmartSpotBrandMarkup } from 'src/app/core/utilities/constants';
import {
  Carrier,
  CarrierScacRestrictionTypes,
  Commodity,
  LoadAuditLogData,
  LoadCarrierScacData,
  LoadCarrierScacRestrictionData,
  ServiceType,
  ShippingLoadCarrierGroupData,
  ShippingLoadDetail,
  ShippingLoadDetailError,
} from 'src/app/shared/models';

@Component({
  selector: 'kbxl-shipping-load-detail',
  templateUrl: './shipping-load-detail.component.html',
  styleUrls: ['./shipping-load-detail.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadDetailComponent implements OnInit, OnChanges {
  @Input() details: ShippingLoadDetail;
  @Input() detailErrors: ShippingLoadDetailError;
  @Input() loadCarrierGroups: ShippingLoadCarrierGroupData[];
  @Input() loadCarrierScacs: LoadCarrierScacData[];
  @Input() loadCarrierScacRestrictions: LoadCarrierScacRestrictionData[];
  @Input() carriers: Carrier[];
  @Input() loadingCarriers: boolean; // loading carriers and/or groups
  @Input() loadingScacs: boolean;
  @Input() loadingScacRestrictions: boolean;
  @Input() commodities: Commodity[];
  @Input() loadingCommodities: boolean;
  @Input() loadAuditLogs: LoadAuditLogData[];
  @Input() loadingAuditLogs: boolean;
  @Input() loadingSmartSpot: boolean;
  @Input() serviceTypes: ServiceType[];
  @Input() loadingServiceTypes: boolean;
  @Output() viewCarrierClicks = new EventEmitter<object>();
  @Output() close = new EventEmitter<ShippingLoadDetail>();
  @Output() updateSmartSpotPrice = new EventEmitter<object>();
  @Output() updateSummaryCard = new EventEmitter<ShippingLoadDetail>();

  @ViewChild('form') form: NgForm;

  set selectedCommodity(commodity: Commodity) {
    if (this.details) {
      this.details.commodity = commodity ? commodity.commodityName : null;
    }
  }
  get selectedCommodity() {
    return this.commodities && this.details
      ? this.commodities.find((_) => _.commodityName.toLowerCase() === this.details.commodity.toLowerCase())
      : null;
  }

  readonlyEditor = true; // hack to prevent underlying quill control from automatically taking focus and scrolling visibile when initialized

  displayGroups: ShippingLoadCarrierGroupData[];
  groupCarriers: Carrier[];
  availableCarriers: Carrier[];

  visibleScacs: LoadCarrierScacData[];
  hiddenScacs: LoadCarrierScacData[];
  carrierGroupSet = false;
  smartSpotLabel = SmartSpotBrandMarkup;

  private _selectedGroups: ShippingLoadCarrierGroupData[];
  set selectedGroups(value: ShippingLoadCarrierGroupData[]) {
    this._selectedGroups = value || [];
    this.updateCarrierLists();
  }
  get selectedGroups() {
    return this._selectedGroups;
  }

  ngOnInit() {
    this.form.valueChanges.subscribe((a: any) => {
      if (this.details) {
        this.details.hasChanges = this.form.dirty;
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes.loadCarrierScacs || changes.loadCarrierScacRestrictions || changes.loadCarrierGroups || changes.carriers) {
      this.updateGroupSelections();
      this.onScacChange();
    }

    if (changes.details) {
      setTimeout(() => {
        this.readonlyEditor = false;
      }, 100); // Hack to get around quill editor autofocus feature/bug
    }
  }

  private updateGroupSelections() {
    if (this.details && this.loadCarrierGroups && this.carriers && this.loadCarrierScacRestrictions) {
      this.orderGroupsAndDefaultSelection();
    }
  }

  private orderGroupsAndDefaultSelection() {
    const stops = this.details ? this.details.loadStops : null;
    const origin = stops && stops.length > 0 ? stops[0] : null;
    const dest = stops && stops.length > 1 ? stops[stops.length - 1] : null;
    const equipmentId = this.details ? this.details.equipmentId.toLowerCase() : null;

    const rankings = this.loadCarrierGroups.map((_) => {
      return {
        group: _,
        rank:
          0 +
          (_.originAddress1 && origin && origin.address1.toLowerCase().indexOf(_.originAddress1.toLowerCase()) >= 0 ? 10000 : 0) +
          (_.originCity && origin && _.originCity.toLowerCase() === origin.city.toLowerCase() ? 1000 : 0) +
          (_.originState && origin && _.originState.toLowerCase() === origin.state.toLowerCase() ? 100 : 0) +
          (_.originCountry && origin && _.originCountry.toLowerCase() === origin.country.toLowerCase() ? 10 : 0) +
          (_.destinationAddress1 && dest && dest.address1.toLowerCase().indexOf(_.destinationAddress1.toLowerCase()) >= 0 ? 10000 : 0) +
          (_.destinationCity && dest && _.destinationCity.toLowerCase() === dest.city.toLowerCase() ? 1000 : 0) +
          (_.destinationState && dest && _.destinationState.toLowerCase() === dest.state.toLowerCase() ? 100 : 0) +
          (_.destinationCountry && dest && _.destinationCountry.toLowerCase() === dest.country.toLowerCase() ? 10 : 0) +
          (_.loadCarrierGroupEquipment &&
          _.loadCarrierGroupEquipment.filter((lcge) => lcge.equipmentId.toLowerCase() === equipmentId).length > 0
            ? 1
            : 0),
      };
    });

    const rankedGroups = rankings.sort((a, b) => (a.rank < b.rank ? 1 : a.rank === b.rank ? 0 : -1)); // descending
    const maxRank = rankedGroups.length > 0 ? rankedGroups[0].rank : null;

    this.displayGroups = rankedGroups.map((_) => _.group);
    if (!this.selectedGroups && (!this.loadCarrierScacs || this.loadCarrierScacs.length === 0)) {
      this.selectedGroups = rankedGroups.filter((_) => _.rank === maxRank).map((_) => _.group);
      this.carrierGroupSet = true;
    } else if (this.loadCarrierScacs && !this.carrierGroupSet) {
      // Set Groups and Carries by Scacs already assigned to load
      const selectedScacs = this.loadCarrierScacs.map((loadCarrierScac) => loadCarrierScac.scac);

      // Set Selected Carriers
      const selectedCarriers: Carrier[] = [];

      this.carriers.forEach((carrier) => {
        const intersectingScacs = carrier.carrierScacs.filter((scac) => selectedScacs.includes(scac));

        if (intersectingScacs.length === carrier.carrierScacs.length) {
          selectedCarriers.push(carrier);
        }
      });

      this.details.selectedCarriers = this.filterCarriersByScacRestrictions(selectedCarriers);

      // Set selected carrier groups. This is a swag as carrier groups might have changed since they were orginally selected
      // Find where all group scacs are included by the Load and select them
      const selectedCarrierGroups: ShippingLoadCarrierGroupData[] = [];

      this.loadCarrierGroups.forEach((group) => {
        const carrierGroupScacs = group.carriers
          .map((carrier) => carrier.carrierScacs)
          .reduce((value, carrierScacs) => value.concat(carrierScacs), []);

        const intersectingScacs = carrierGroupScacs.filter((scac) => selectedScacs.includes(scac));

        if (intersectingScacs.length === carrierGroupScacs.length) {
          selectedCarrierGroups.push(group);
        }
      });

      this.selectedGroups = selectedCarrierGroups;
    }
  }

  private updateCarrierLists() {
    if (this.loadCarrierGroups && this.loadCarrierGroups.length > 0) {
      const allGroupCarriersObj = (this.selectedGroups || []).reduce((value, item) => {
        item.carriers.forEach((carrier) => (value[carrier.carrierId] = carrier));
        return value;
      }, <{ [s: string]: Carrier }>{});

      const allGroupCarriers = Object.keys(allGroupCarriersObj).map((_) => allGroupCarriersObj[_]);
      if (!allGroupCarriers || allGroupCarriers.length === 0) {
        this.details.selectedGroupCarriers = null;
        this.groupCarriers = null;
        this.availableCarriers = this.filterCarriersByScacRestrictions(this.carriers);
        return;
      }

      // Use to filter carrier group carriers for faster performance
      const allCarriersObj = (this.carriers || []).reduce((value, carrier) => {
        value[carrier.carrierId] = carrier;
        return value;
      }, {});

      // keep any selected carriers that are still in the list
      // let selections = (this.details.selectedGroupCarriers || []).filter(_ => allGroupCarriersObj.hasOwnProperty(_.carrierId));
      // add any carriers that were not in the list but now are due to a new group being selected
      // let selections = this.groupCarriers ? allGroupCarriers.filter(_ => this.groupCarriers.indexOf(_) < 0) : allGroupCarriers;

      // filter group carriers to only carriers that are active
      const activeGroupCarriers = this.filterCarriersByScacRestrictions(
        allGroupCarriers.filter((carrier) => allCarriersObj.hasOwnProperty(carrier.carrierId))
      );

      this.groupCarriers = activeGroupCarriers;
      this.details.selectedGroupCarriers = activeGroupCarriers;

      // remove any selected carriers that were placed into the group carrier list and update the available carriers
      this.details.selectedCarriers = this.filterCarriersByScacRestrictions(
        (this.details.selectedCarriers || []).filter((_) => !allGroupCarriersObj.hasOwnProperty(_.carrierId))
      );
      this.availableCarriers = (this.carriers || []).filter((_) => !allGroupCarriersObj.hasOwnProperty(_.carrierId));
    } else {
      this.availableCarriers = this.carriers;
      if (!this.details.selectedCarriers) {
        this.details.selectedCarriers = this.filterCarriersByScacRestrictions(this.carriers);
      }
    }

    this.availableCarriers = this.filterCarriersByScacRestrictions(this.availableCarriers);
  }

  private filterCarriersByScacRestrictions(carriersToFilter: Carrier[]): Carrier[] {
    if (this.loadCarrierScacRestrictions && carriersToFilter) {
      const useOnlyScacs = this.loadCarrierScacRestrictions
        .filter((x) => x.loadCarrierScacRestrictionTypeId === CarrierScacRestrictionTypes.UseOnly)
        .map((x) => x.scac);
      const doNotUseScacs = this.loadCarrierScacRestrictions
        .filter((x) => x.loadCarrierScacRestrictionTypeId === CarrierScacRestrictionTypes.DoNotUse)
        .map((x) => x.scac);

      const validCarriers: Carrier[] = [];
      carriersToFilter.forEach((carrier) => {
        const carrierScacs = carrier.carrierScacs.filter(
          (scac) =>
            (useOnlyScacs.length === 0 || useOnlyScacs.includes(scac)) && (doNotUseScacs.length === 0 || !doNotUseScacs.includes(scac))
        );

        if (carrierScacs.length > 0) {
          const newCarrier = { ...carrier, carrierScacs: carrierScacs };
          validCarriers.push(newCarrier);
        }
      });

      return validCarriers;
    }

    return carriersToFilter;
  }

  private updateScacVisibility() {
    const rate = this.details ? this.details.lineHaulRate : null;

    const sortedScacs = this.details ? (this.loadCarrierScacs || []).sort(this.sortCarrierStacs) : [];

    const selectedCarrierScacs = (this.details.selectedCarriers || [])
      .concat(this.details.selectedGroupCarriers || [])
      .map((x) => x.carrierScacs)
      .reduce((input, output) => input.concat(output), []);

    const filterdScacs = sortedScacs.filter((scac) => selectedCarrierScacs.indexOf(scac.scac) > -1);

    this.visibleScacs = filterdScacs.filter((_) => (_.contractRate || 0) <= (rate || 0));
    this.hiddenScacs = filterdScacs.filter((_) => (_.contractRate || 0) > (rate || 0));
  }

  private sortCarrierStacs(a: LoadCarrierScacData, b: LoadCarrierScacData) {
    if (a.contractRate === null && b.contractRate !== null) {
      return 1;
    } else if (b.contractRate === null && a.contractRate !== null) {
      return -1;
    }
    if (a.contractRate > b.contractRate) {
      return 1;
    } else if (a.contractRate < b.contractRate) {
      return -1;
    } else if (a.scac > b.scac) {
      return 1;
    } else if (a.scac < b.scac) {
      return -1;
    }
    return 0;
  }

  closeClicked() {
    if (this.details) {
      this.close.emit(this.details);
    }
  }

  viewCarrierClicksClicked() {
    this.viewCarrierClicks.emit(null);
  }

  viewChart() {
    alert('TODO: get mockup of chart and implement');
  }

  showErrorSummary() {
    return (
      this.detailErrors &&
      (this.detailErrors.root ||
        this.detailErrors.load ||
        this.detailErrors.lineHaulRate ||
        this.detailErrors.commodity ||
        this.detailErrors.shippersFSC)
    );
  }

  onCommodityChange() {
    this.triggerSmartSpotPriceUpdate();
  }

  triggerSmartSpotPriceUpdate() {
    if (
      this.details &&
      this.loadCarrierGroups &&
      this.carriers &&
      this.loadCarrierScacs &&
      this.selectedGroups &&
      (this.details.selectedCarriers || this.details.selectedGroupCarriers)
    ) {
      const carrierIds = (this.details.selectedCarriers || []).concat(this.details.selectedGroupCarriers || []).map((x) => x.carrierId);

      this.updateSmartSpotPrice.emit({ details: this.details, carrierIds: carrierIds });
    }
  }

  onScacChange() {
    this.updateScacVisibility();
    this.triggerSmartSpotPriceUpdate();

    if (this.selectedGroups) {
      this.details.allCarriersPosted = this.selectedGroups.length === 0 && this.carriers.length === this.details.selectedCarriers.length;
      this.details.carrierGroupIds = this._selectedGroups.map((carrierGroup) => carrierGroup.loadCarrierGroupId);
    }
  }

  onRateChange() {
    this.updateSummaryCard.emit(this.details);
  }
}
