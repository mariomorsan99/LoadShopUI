import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';

import { ShippingLoadDetail, SmartSpotPrice } from 'src/app/shared/models';
import { Dictionary } from '@ngrx/entity';

@Component({
  selector: 'kbxl-shipping-load-home-header',
  templateUrl: './shipping-load-home-header.component.html',
  styleUrls: ['./shipping-load-home-header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShippingLoadHomeHeaderComponent {
  @Input() selectedLoads: ShippingLoadDetail[];
  @Input() loadingAnySmartSpotPrices: boolean;
  @Input() smartSpotPrices: Dictionary<SmartSpotPrice>;
  @Input() allowManualLoadCreation: boolean;

  @Output() postLoads = new EventEmitter<ShippingLoadDetail[]>();
  @Output() createLoad = new EventEmitter<ShippingLoadDetail>();

  postClicked() {
    // post the new SmartSpot on the posted load
    this.selectedLoads.forEach(load => {
      const ssp = this.smartSpotPrices ? this.smartSpotPrices[load.loadId] : null;
      load.smartSpotRate = ssp ? ssp.price : load.smartSpotRate;
      load.datGuardRate = ssp ? ssp.datGuardRate : load.datGuardRate;
      load.machineLearningRate = ssp ? ssp.machineLearningRate : load.machineLearningRate;
    });
    this.postLoads.emit(this.selectedLoads);
  }

  createClicked() {
    this.createLoad.emit();
  }
}
