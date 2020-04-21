import { Component, ChangeDetectionStrategy, Input, OnInit, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { Store, select } from '@ngrx/store';
import {
  AdminState,
  ShipperProfileLoadSourceSystemOwnerAction,
  ShipperProfileLoadShipperMappingsAction,
  loadingSourceSystemOwners,
  loadingShipperMappings,
  getShipperMappings,
  getSourceSystemOwners,
  ShipperProfileCreateShipperMappingAction,
  ShipperProfileUpdateShipperMappingAction,
  savingShipperMapping,
} from 'src/app/admin/store';
import { map, distinctUntilChanged } from 'rxjs/operators';
import { Observable, combineLatest, Subscription } from 'rxjs';
import { LoadshopShipperMapping, defaultLoadshopShipperMapping } from 'src/app/shared/models/loadshop-shipper-mapping';
import { SelectItem } from 'primeng/api';

@Component({
  selector: 'kbxl-shipper-mapping-modal',
  templateUrl: './shipper-mapping-modal.component.html',
  styleUrls: ['./shipper-mapping-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShipperMappingModalComponent implements OnInit, OnChanges, OnDestroy {
  @Input() public ownerId: string;
  public shipperMappings$: Observable<LoadshopShipperMapping[]>;
  public sourceSystemOwners$: Observable<Map<string, string[]>>;
  public processing$: Observable<boolean>;

  public editingRow = false;
  public availableSourceSystems: SelectItem[] = [];
  private sourceSystemSub: Subscription;
  private clonedMappings: LoadshopShipperMapping[] = [];

  constructor(private adminStore: Store<AdminState>) {
    // Observables
    this.shipperMappings$ = this.adminStore.pipe(map(getShipperMappings), distinctUntilChanged());
    this.sourceSystemOwners$ = this.adminStore.pipe(map(getSourceSystemOwners), distinctUntilChanged());
  }

  ngOnInit() {
    this.processing$ = combineLatest(
      this.adminStore.pipe(select(loadingSourceSystemOwners)),
      this.adminStore.pipe(select(loadingShipperMappings)),
      this.adminStore.pipe(select(savingShipperMapping))
    ).pipe(map(args => args[0] || args[1] || args[2]));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && changes.ownerId && changes.ownerId.currentValue !== changes.ownerId.previousValue) {
      // Dispatch
      this.adminStore.dispatch(new ShipperProfileLoadSourceSystemOwnerAction({ ownerId: this.ownerId }));
      this.adminStore.dispatch(new ShipperProfileLoadShipperMappingsAction({ ownerId: this.ownerId }));
      // Subscriptions
      this.sourceSystemSub = combineLatest(this.sourceSystemOwners$, this.shipperMappings$).subscribe(([ssos, mappings]) => {
        if (ssos && mappings) {
          const sourceSystems = ssos[this.ownerId];
          this.availableSourceSystems = [];
          if (sourceSystems) {
            sourceSystems.forEach((x: string) => {
              const r = mappings.find(y => y.sourceSystem === x);
              if (!r) {
                this.availableSourceSystems.push({ label: x, value: x });
              }
            });
          }
        }
      });
    }
  }

  ngOnDestroy() {
    if (this.sourceSystemSub) {
      this.sourceSystemSub.unsubscribe();
      this.sourceSystemSub = null;
    }
  }

  addRecord(e: any) {
    const mapping: LoadshopShipperMapping = {
      ...defaultLoadshopShipperMapping,
      ownerId: this.ownerId,
      sourceSystem: e.value,
      loadshopShipperId: this.ownerId,
    };
    this.adminStore.dispatch(new ShipperProfileCreateShipperMappingAction({ mapping: mapping }));
  }

  onRowEditInit(mapping: LoadshopShipperMapping) {
    this.clonedMappings[mapping.loadshopShipperMappingId] = { ...mapping };
    this.editingRow = true;
  }

  onRowEditSave(mapping: LoadshopShipperMapping) {
    delete this.clonedMappings[mapping.loadshopShipperMappingId];
    this.editingRow = false;
    this.adminStore.dispatch(new ShipperProfileUpdateShipperMappingAction({ mapping: mapping }));
  }

  onRowEditCancel(mapping: LoadshopShipperMapping, index: number, s: LoadshopShipperMapping[]) {
    s[index] = this.clonedMappings[mapping.loadshopShipperMappingId];
    delete this.clonedMappings[mapping.loadshopShipperMappingId];
    this.editingRow = false;
  }
}
