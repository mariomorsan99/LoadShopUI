import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { ShippingLoadDetail } from 'src/app/shared/models';

@Component({
  selector: 'kbxl-shipping-load-cards',
  templateUrl: './shipping-load-cards.component.html',
  styleUrls: ['./shipping-load-cards.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadCardsComponent {
  @Input() loads: ShippingLoadDetail[];
  @Input() selectedLoads: ShippingLoadDetail[];

  @Output() loadSelected = new EventEmitter<ShippingLoadDetail>();
  @Output() loadUnselected = new EventEmitter<ShippingLoadDetail>();
  @Output() removeLoad = new EventEmitter<ShippingLoadDetail>();
  @Output() deleteLoad = new EventEmitter<ShippingLoadDetail>();

  isLoadSelected = (load: ShippingLoadDetail) => !!(this.selectedLoads || []).find(_ => _.loadId === load.loadId);

  constructor(private route: ActivatedRoute, private router: Router, private confirmationService: ConfirmationService) {}

  onRowSelect(load: ShippingLoadDetail) {
    if (this.isLoadSelected(load)) {
      this.loadUnselected.emit(load);
    } else {
      this.loadSelected.emit(load);
    }
  }

  edit(load: any) {
    if (load && load.manuallyCreated && load.loadId) {
      if (load.onLoadshop) {
        this.confirmationService.confirm({
          // tslint:disable-next-line:max-line-length
          message: `Are you sure you want to edit this load?<br/><br/>It will be removed from the Marketplace and need to be reposted after saving.`,
          accept: () => {
            this.router.navigate(['edit', load.loadId], { relativeTo: this.route });
          },
        });
      } else {
        this.router.navigate(['edit', load.loadId], { relativeTo: this.route });
      }
    }
  }

  removeClicked(load: ShippingLoadDetail) {
    this.removeLoad.emit(load);
  }

  deleteClicked(load: ShippingLoadDetail) {
    this.deleteLoad.emit(load);
  }
}
