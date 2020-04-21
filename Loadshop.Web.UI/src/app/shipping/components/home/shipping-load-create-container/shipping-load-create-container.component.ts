import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService, TreeNode } from 'primeng/api';
import { combineLatest, Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import {
  CoreState,
  getAppointmentSchedulerConfirmationTypes,
  getCommodities,
  getEquipment,
  getLoadingAppointmentSchedulerConfirmationTypes,
  getLoadingCommodities,
  getLoadingEquipment,
  getLoadingServiceTypes,
  getLoadingStates,
  getLoadingTransporationModes,
  getLoadingUnitsOfMeasure,
  getServiceTypes,
  getStates,
  getTransporationModes,
  getUnitsOfMeasure,
  getQuickQuoteCreateOrderDetails,
  SmartSpotClearCreateOrderFromQuote,
} from 'src/app/core/store';
import { GuidEmpty } from 'src/app/core/utilities/constants';
import {
  AppointmentSchedulerConfirmationType,
  Commodity,
  defaultLoadLineItem,
  defaultTransportationMode,
  Equipment,
  LoadLineItem,
  LoadStop,
  ServiceType,
  State,
  StopTypes,
  TransportationMode,
  UnitOfMeasure,
} from 'src/app/shared/models';
import { LoadContact } from 'src/app/shared/models/load-contact';
import { LoadStopContact } from 'src/app/shared/models/load-stop-contact';
import { OrderEntryForm } from 'src/app/shared/models/order-entry-form';
import { defaultOrderEntryDeliveryStop, OrderEntryLoadStop } from 'src/app/shared/models/order-entry-load-stop';
import { ValidationProblemDetails } from 'src/app/shared/models/validation-problem-details';
import {
  getOrderEntryForm,
  getOrderEntryLoadingDetails,
  getOrderEntryProblemDetails,
  getOrderEntrySavedId,
} from 'src/app/shipping/store/selectors/order-entry.selectors';
import { CanDeactivateGuard } from '../../../../core/guards/can-deactivate.guard';
import {
  OrderEntryClearErrorsAction,
  OrderEntryCreateLoadAction,
  OrderEntryGetLoadAction,
  OrderEntryResetSavedAction,
  OrderEntryUpdateLoadAction,
  ShippingLoadDetailLoadAllAction,
  ShippingState,
} from '../../../store';

@Component({
  selector: 'kbxl-shipping-load-create-container',
  templateUrl: './shipping-load-create-container.component.html',
  styleUrls: ['./shipping-load-create-container.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShippingLoadCreateContainerComponent implements OnInit, OnDestroy, CanDeactivateGuard {
  filter$: Observable<any>;
  loading$: Observable<boolean>;

  problemDetails: ValidationProblemDetails;
  defaultLoadStopContactErrorLabels = {
    email: false,
    phoneNumber: false,
    display: false,
    firstName: false,
    lastName: false,
  };
  defaultLoadLineItemErrorLabels = {
    loadLineItemNumber: false,
    quantity: false,
    unitOfMeasure: false,
    weight: false,
    customerPurchaseOrder: false,
    pickupStopNumber: false,
    deliveryStopNumber: false,
  };
  defaultLoadStopErrorLabels = {
    city: false,
    state: false,
    address1: false,
    locationName: false,
    postalCode: false,
    schedulerConfirmationType: false,
    pickupTo: false,
    contactLabels: [{ ...this.defaultLoadStopContactErrorLabels }],
    lineItemLabels: [{ ...this.defaultLoadLineItemErrorLabels }],
  };
  /**
   * Every contact has to be unique flower, so we have display on some
   * and first name and last name on others.  Eventually these should be
   * consolidated, since the UI always has first name and last name
   */
  defaultLoadContactErrorLabels = {
    email: false,
    phoneNumber: false,
    display: false,
    firstName: false,
    lastName: false,
  };
  defaultErrorLabels = {
    referenceLoadDisplay: false,
    commodity: false,
    equipment: false,
    shipperPickupNumber: false,
    transportationMode: false,
    services: false,
    contacts: false,

    // Single order contact on the form by default
    contactLabels: [{ ...this.defaultLoadContactErrorLabels }],

    // Two load stops on the form by default; require error label objects for each
    loadStopLabels: [{ ...this.defaultLoadStopErrorLabels }, { ...this.defaultLoadStopErrorLabels }],
  };
  errorLabels = this.defaultErrorLabels;
  errorSummary = null;
  errorCount = 0;

  form: FormGroup;
  orderEntry: OrderEntryForm;
  isExistingLoad = false;
  pickupStopNumbers: { stopNbr: number }[] = [];

  commodities$: Observable<Commodity[]>;
  loadingCommodities$: Observable<boolean>;
  equipment$: Observable<Equipment[]>;
  loadingEquipment$: Observable<boolean>;
  unitsOfMeasure$: Observable<UnitOfMeasure[]>;
  loadingUnitsOfMeasure$: Observable<boolean>;
  transportationModes$: Observable<TransportationMode[]>;
  loadingTransportationModes$: Observable<boolean>;
  serviceTypes$: Observable<ServiceType[]>;
  loadingServiceTypes$: Observable<boolean>;
  schedulerConfirmationTypes$: Observable<AppointmentSchedulerConfirmationType[]>;
  loadingSchedulerConfirmationTypes$: Observable<boolean>;
  states$: Observable<State[]>;
  loadingStates$: Observable<boolean>;
  selectedEquipment: TreeNode;
  apiRequest: string = null;

    private loadMap = [
    { urn: '', formControlName: '' },
    { urn: 'ReferenceLoadDisplay', formControlName: 'referenceLoadDisplay' },
    { urn: 'Commodity', formControlName: 'commodity' },
    { urn: 'EquipmentType', formControlName: 'equipment' },
    { urn: 'Contacts', formControlName: 'contacts' },
    { urn: 'LoadStops', formControlName: 'loadStops' },
    { urn: 'LineItems', formControlName: '' }, // for now just display the error, we may change it to be required per delivery stop
  ];
  private contactMap = [
    { urn: '', formControlName: '' },
    { urn: 'Email', formControlName: 'email' },
    { urn: 'Phone', formControlName: 'phoneNumber' },
    { urn: 'EmailOrPhoneNumber', formControlName: 'phoneNumber' },
    { urn: 'Display', formControlName: 'firstName' },
    { urn: 'FirstName', formControlName: 'firstName' },
    { urn: 'LastName', formControlName: 'lastName' },
  ];
  private loadStopMap = [
    { urn: '', formControlName: '' },
    { urn: 'StopNbr', formControlName: 'stopNbr' },
    { urn: 'StopType', formControlName: 'stopType' },
    { urn: 'City', formControlName: 'city' },
    { urn: 'State', formControlName: 'state' },
    { urn: 'Country', formControlName: 'country' },
    { urn: 'Address1', formControlName: 'address1' },
    { urn: 'LocationName', formControlName: 'locationName' },
    { urn: 'PostalCode', formControlName: 'postalCode' },
    { urn: 'StopType', formControlName: 'stopType' },
    { urn: 'AppointmentSchedulingCode', formControlName: 'schedulerConfirmationType' },
    { urn: 'AppointmentConfirmationCode', formControlName: 'schedulerConfirmationType' },
    { urn: 'EarlyDtTm', formControlName: 'earlyDtTm' },
    { urn: 'EarlyTime', formControlName: 'earlyTime' },
    { urn: 'LateDtTm', formControlName: 'lateDtTm' },
    { urn: 'LateTime', formControlName: 'lateTime' },
    { urn: 'Contacts', formControlName: 'contacts' },
    { urn: 'LineItems', formControlName: 'lineItems' },
  ];
  private lineItemMap = [
    { urn: '', formControlName: '' },
    { urn: 'Quantity', formControlName: 'quantity' },
    { urn: 'Weight', formControlName: 'weight' },
    { urn: 'Volume', formControlName: 'volume' },
    { urn: 'UnitOfMeasure', formControlName: 'unitOfMeasure' },
    { urn: 'PickupStopNumber', formControlName: 'pickupStopNumber' },
    { urn: 'DeliveryStopNumber', formControlName: 'deliveryStopNumber' },
  ];

  constructor(
    private shippingStore: Store<ShippingState>,
    private coreStore: Store<CoreState>,
    private confirmationService: ConfirmationService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {}

  /**
   * https://medium.com/@stodge/ngrx-common-gotchas-8f59f541e47c
   * Required to prevent memory leaks when subscribing to ngrx store for purposes
   * other than dumping to an HTML template with an async pipe
   */
  private ngUnsubscribe: Subject<void> = new Subject<void>();
  ngOnDestroy() {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();

    this.coreStore.dispatch(new SmartSpotClearCreateOrderFromQuote());
  }

  ngOnInit() {
    this.loading$ = this.shippingStore.pipe(select(getOrderEntryLoadingDetails));
    this.route.params.pipe(map(params => params)).subscribe(params => {
      let id = '';
      if (params.id) {
        id = params.id;
      }
      this.shippingStore.dispatch(new OrderEntryGetLoadAction(id));
    });

    this.commodities$ = this.coreStore.pipe(select(getCommodities));
    this.loadingCommodities$ = this.coreStore.pipe(select(getLoadingCommodities));
    this.equipment$ = this.coreStore.pipe(select(getEquipment));
    this.loadingEquipment$ = this.coreStore.pipe(select(getLoadingEquipment));
    this.unitsOfMeasure$ = this.coreStore.pipe(select(getUnitsOfMeasure));
    this.loadingUnitsOfMeasure$ = this.coreStore.pipe(select(getLoadingUnitsOfMeasure));
    this.transportationModes$ = this.coreStore.pipe(select(getTransporationModes));
    this.loadingTransportationModes$ = this.coreStore.pipe(select(getLoadingTransporationModes));
    this.serviceTypes$ = this.coreStore.pipe(select(getServiceTypes));
    this.loadingServiceTypes$ = this.coreStore.pipe(select(getLoadingServiceTypes));
    this.schedulerConfirmationTypes$ = this.coreStore.pipe(select(getAppointmentSchedulerConfirmationTypes));
    this.loadingSchedulerConfirmationTypes$ = this.coreStore.pipe(select(getLoadingAppointmentSchedulerConfirmationTypes));
    this.states$ = this.coreStore.pipe(select(getStates));
    this.loadingStates$ = this.coreStore.pipe(select(getLoadingStates));

    combineLatest(
      this.shippingStore.select(getOrderEntryForm),
      this.coreStore.select(getAppointmentSchedulerConfirmationTypes),
      this.coreStore.select(getUnitsOfMeasure),
      this.coreStore.pipe(select(getQuickQuoteCreateOrderDetails))
    ).pipe(takeUntil(this.ngUnsubscribe))
    .subscribe(([orderEntry, schedConfTypes, unitsOfMeasure, qqDetails]) => {
      this.orderEntry = orderEntry;
      if (this.orderEntry.loadId && this.orderEntry.loadId.length > 0) {
        this.isExistingLoad = true;
      }

      const allDataLoaded = !!orderEntry && !!schedConfTypes && !!unitsOfMeasure;
      if (!this.isExistingLoad && allDataLoaded && qqDetails) {
        this.orderEntry.equipment = qqDetails.equipment ? qqDetails.equipment.equipmentId : null;
        this.orderEntry.equipmentDesc = qqDetails.equipment ? qqDetails.equipment.equipmentDesc : null;
        this.orderEntry.categoryEquipmentDesc = qqDetails.equipment ? qqDetails.equipment.categoryEquipmentDesc : null;
        this.orderEntry.loadStops[0].city = qqDetails.origin ? qqDetails.origin.city : null;
        this.orderEntry.loadStops[0].state = qqDetails.origin ? qqDetails.origin.stateAbbrev : null;
        this.orderEntry.loadStops[0].stateName = qqDetails.origin ? qqDetails.origin.state : null;
        this.orderEntry.loadStops[0].postalCode = qqDetails.origin ? qqDetails.origin.postalCode : null;
        this.orderEntry.loadStops[0].country = qqDetails.origin ? qqDetails.origin.country : null;
        this.orderEntry.loadStops[1].city = qqDetails.destination ? qqDetails.destination.city : null;
        this.orderEntry.loadStops[1].state = qqDetails.destination ? qqDetails.destination.stateAbbrev : null;
        this.orderEntry.loadStops[1].stateName = qqDetails.destination ? qqDetails.destination.state : null;
        this.orderEntry.loadStops[1].postalCode = qqDetails.destination ? qqDetails.destination.postalCode : null;
        this.orderEntry.loadStops[1].country = qqDetails.destination ? qqDetails.destination.country : null;
        this.orderEntry.loadStops[0].lateDate = qqDetails.pickupDate;
        this.orderEntry.lineItems[0].weight = qqDetails.weight;
      }

      this.buildForm(orderEntry, schedConfTypes, unitsOfMeasure);
      this.shippingStore.dispatch(new OrderEntryClearErrorsAction());

      // Don't try to set errors until the form is completely built
      this.shippingStore
        .select(getOrderEntryProblemDetails)
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe(problems => {
          this.problemDetails = problems;
          this.setErrors();
        });
    });

    // We don't actually save anything about the form in state, so
    // subscribing to changes in the order entry form doesn't do us any
    // good.  As such, we check this saved state and rebuild our form
    // from the same default initial state of the order entry form
    combineLatest(this.shippingStore.select(getOrderEntryForm), this.shippingStore.select(getOrderEntrySavedId))
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(([orderEntry, savedId]) => {
        if (savedId && savedId !== GuidEmpty) {
          this.confirmationService.confirm({
            message: 'Would you like to go back to the Post tab or enter another order?',
            acceptLabel: 'Go to Post Tab',
            rejectLabel: 'Remain on Order Entry',
            accept: () => {
              // Reload all Shipping Post tab loads, so that the new one is available to select
              this.shippingStore.dispatch(new ShippingLoadDetailLoadAllAction());
              this.shippingStore.dispatch(new OrderEntryResetSavedAction());
              this.router.navigate(['/shipping/home'], { queryParams: { loadids: savedId, sep: ';' } });
              this.coreStore.dispatch(new SmartSpotClearCreateOrderFromQuote());
            },
            reject: () => {
              this.orderEntry = orderEntry;
              this.buildForm(orderEntry, null);
              this.shippingStore.dispatch(new OrderEntryResetSavedAction());
              // reset the form with the user profile specifics
              this.shippingStore.dispatch(new OrderEntryGetLoadAction(''));
              this.coreStore.dispatch(new SmartSpotClearCreateOrderFromQuote());
            },
          });
        }
      });
  }

  get orderContacts() {
    return this.form.controls['contacts'] as FormArray;
  }

  get loadStops() {
    return this.form.controls['loadStops'] as FormArray;
  }

  addOrderContact() {
    this.orderContacts.push(
      this.fb.group({
        firstName: '',
        lastName: '',
        phoneNumber: '',
        email: '',
      })
    );
  }

  deleteOrderContact(index: number) {
    this.orderContacts.removeAt(index);
  }

  setErrors() {
    this.errorSummary = '';
    this.errorCount = 0;
    this.setLoadErrors(this.problemDetails ? this.problemDetails.errors || {} : {});
  }

  setFormGroupErrors(formObject: FormGroup | FormArray, urnPrefix: string, errorMap: { urn: string; formControlName: string }[], errors) {
    formObject.setErrors(null);
    Object.keys(formObject.controls).forEach(key => {
      formObject.controls[key].setErrors(null);
    });

    for (const entry of errorMap) {
      const currentUrn = urnPrefix + (entry.urn && entry.urn.length ? ':' + entry.urn : '');
      const name = entry.formControlName;
      const controlErrors = errors ? errors[currentUrn] : null;

      // Track the full list of errors
      if (controlErrors) {
        for (const error of controlErrors) {
          if (error) {
            this.errorSummary += `${error.trim()}\n`;
            this.errorCount++;
          }
        }

        if (name != null) {
          if (name.length === 0) {
            formObject.setErrors(controlErrors);
          } else {
            formObject.get(name).setErrors(controlErrors);
          }
        }
      }
    }
  }

  setLoadErrors(errors) {
    const urnRoot = 'urn:root';
    this.setFormGroupErrors(this.form, urnRoot, this.loadMap, errors);

    this.setContactErrors(urnRoot, errors, this.form.get('contacts') as FormArray);
    this.setLoadStopErrors(urnRoot, errors);
  }

  setContactErrors(urnPrefix: string, errors, contacts: FormArray) {
    for (let i = 0; i < contacts.length; i++) {
      const contactUrnPrefix = `${urnPrefix}:Contacts:${i}`;
      this.setFormGroupErrors(contacts.controls[i] as FormGroup, contactUrnPrefix, this.contactMap, errors);
    }
  }

  setLoadStopErrors(urnPrefix: string, errors) {
    const loadStops = this.loadStops;
    let stopIndex = 0;
    let lineItemIndex = 0;

    for (let i = 0; i < loadStops.length; i++) {
      const stopUrnPrefix = `${urnPrefix}:LoadStops:${stopIndex}`;
      const stopGroup = loadStops.controls[i] as FormGroup;
      this.setFormGroupErrors(stopGroup, stopUrnPrefix, this.loadStopMap, errors);
      stopIndex++;

      const lineItems = stopGroup.controls['lineItems'] as FormArray;
      for (let j = 0; j < lineItems.length; j++) {
        const lineItemUrnPrefix = `${urnPrefix}:LineItems:${lineItemIndex}`;
        this.setFormGroupErrors(lineItems.controls[j] as FormGroup, lineItemUrnPrefix, this.lineItemMap, errors);
        lineItemIndex++;
      }

      const contacts = stopGroup.controls['contacts'] as FormArray;
      for (let j = 0; j < contacts.length; j++) {
        const contactUrnPrefix = `${stopUrnPrefix}:Contacts:${j}`;
        this.setFormGroupErrors(contacts.controls[j] as FormGroup, contactUrnPrefix, this.contactMap, errors);
      }
    }
  }

  buildForm(
    orderEntry: OrderEntryForm,
    schedConfTypes: AppointmentSchedulerConfirmationType[] = null,
    unitsOfMeasure: UnitOfMeasure[] = null
  ) {
    this.form = this.fb.group({
      loadId: [orderEntry.loadId],
      referenceLoadDisplay: [orderEntry.referenceLoadDisplay],
      referenceLoadId: [orderEntry.referenceLoadId],
      commodity: { commodityName: orderEntry.commodity },
      equipment: { equipmentId: orderEntry.equipment, equipmentDesc: orderEntry.equipmentDesc },
      contacts: this.buildLoadContacts(orderEntry.contacts),
      shipperPickupNumber: [orderEntry.shipperPickupNumber],
      transportationMode: [defaultTransportationMode],
      services: [orderEntry.services],
      loadStops: this.buildLoadStopsUI(orderEntry.loadStops, orderEntry.lineItems, schedConfTypes, unitsOfMeasure),
      specialInstructions: orderEntry.specialInstructions,
      lineHaulRate: [orderEntry.lineHaulRate],
      fuelRate: [orderEntry.fuelRate],
      weight: [orderEntry.weight],
      cube: [orderEntry.cube],
    });

    this.selectedEquipment = null;
    if (orderEntry.equipment) {
      this.selectedEquipment = {
        label: orderEntry.categoryEquipmentDesc,
        selectable: true,
        key: orderEntry.equipment,
      };
    }

    this.pickupStopNumbers = [];
    orderEntry.loadStops.forEach(_ => {
      if (_.stopType === StopTypes[StopTypes.Pickup]) {
        this.pickupStopNumbers.push({ stopNbr: _.stopNbr });
      }
    });

    setTimeout(function() {
      window.scroll(0, 0);
    }, 100);
  }

  buildLoadContacts(contacts: LoadContact[]): FormArray {
    if (!contacts || contacts.length <= 0) {
      contacts = [];
    }

    const controls = contacts.map(c => {
      return this.fb.group({
        firstName: c.firstName,
        lastName: c.lastName,
        phoneNumber: c.phoneNumber,
        email: c.email,
      });
    });
    return this.fb.array(controls);
  }

  buildLoadStopContacts(contacts: LoadStopContact[]): FormArray {
    if (!contacts || contacts.length <= 0) {
      contacts = [];
    }

    const controls = contacts.map(c => {
      return this.fb.group({
        firstName: c.firstName,
        lastName: c.lastName,
        phoneNumber: c.phoneNumber,
        email: c.email,
      });
    });
    return this.fb.array(controls);
  }

  buildLoadLineItems(lineItems: LoadLineItem[], unitsOfMeasure: UnitOfMeasure[] = null): FormArray {
    if (!lineItems || lineItems.length <= 0) {
      lineItems = [];
    }

    const controls = lineItems.map(l => {
      let unitOfMeasureName = null;
      if (unitsOfMeasure && unitsOfMeasure.length) {
        const uom = unitsOfMeasure.find(x => x.unitOfMeasureId === l.unitOfMeasureId);
        unitOfMeasureName = uom ? uom.name : null;
      }
      return this.fb.group({
        loadLineItemNumber: l.loadLineItemNumber,
        quantity: l.quantity,
        unitOfMeasure: unitOfMeasureName,
        weight: l.weight,
        customerPurchaseOrder: l.customerPurchaseOrder,
        pickupStopNumber: l.pickupStopNumber,
        deliveryStopNumber: l.deliveryStopNumber,
      });
    });
    return this.fb.array(controls);
  }

  /**
   * Build a list of load pairs to match the UI layout, where for each
   * row of the UI there is 0 or 1 pickups at index 0, and 0 or 1 deliveries
   * at index 2.  This way the UI can be built by looping through the list of rows.
   * @param loadStops List of all LoadStops, pickups first, followed by deliveries
   */
  buildLoadStopsUI(
    loadStops: OrderEntryLoadStop[],
    lineItems: LoadLineItem[],
    schedConfTypes: AppointmentSchedulerConfirmationType[] = null,
    unitsOfMeasure: UnitOfMeasure[] = null
  ): FormArray {
    const allStops = [];
    for (let x = 0; x < loadStops.length; x++) {
      const stopFormGroup = this.buildLoadStopFormGroup(loadStops[x], lineItems, schedConfTypes, unitsOfMeasure);
      allStops.push(stopFormGroup);
    }

    return this.fb.array(allStops);
  }

  buildLoadStopFormGroup(
    s: OrderEntryLoadStop,
    lineItems: LoadLineItem[],
    schedConfTypes: AppointmentSchedulerConfirmationType[] = null,
    unitsOfMeasure: UnitOfMeasure[] = null
  ): FormGroup {
    const stopLineItems =
      lineItems && s.stopType === StopTypes[StopTypes.Delivery] ? lineItems.filter(_ => _.deliveryStopNumber === s.stopNbr) : null;
    let schedulerConfirmationType = null;
    if (schedConfTypes && schedConfTypes.length > 0) {
      const confCode = s.appointmentConfirmationCode;
      const schedCode = s.appointmentSchedulingCode;
      if (confCode && schedCode) {
        schedulerConfirmationType = schedConfTypes.find(
          item => item.appointmentConfirmationCode === confCode && item.appointmentSchedulingCode === schedCode
        );
      }
    }
    return this.fb.group({
      loadStopId: s.loadStopId,
      loadId: s.loadId,
      stopNbr: s.stopNbr,
      city: s.city,
      state: { abbreviation: s.state, name: s.stateName },
      country: { name: s.country },
      latitude: s.latitude,
      longitude: s.longitude,
      earlyDtTm: s.earlyDtTm,
      earlyDate: s.earlyDate,
      earlyTime: s.earlyTime,
      lateDtTm: s.lateDtTm,
      lateDate: s.lateDate,
      lateTime: s.lateTime,
      apptType: s.apptType,
      instructions: s.instructions,
      schedulerConfirmationType: schedulerConfirmationType,
      locationName: s.locationName,
      addressAutoComplete: { address: s.address1 },
      address1: s.address1,
      address2: s.address2,
      address3: s.address3,
      postalCode: s.postalCode,
      isLive: s.isLive ? 'true' : 'false',
      stopType: s.stopType,
      contacts: this.buildLoadStopContacts(s.contacts),
      lineItems: this.buildLoadLineItems(stopLineItems, unitsOfMeasure),
    });
  }

  isStopContactEmpty(c: LoadStopContact): boolean {
    return !c.firstName && !c.lastName && !c.phoneNumber && !c.email;
  }

  isApiLoadContactEmpty(c: any): boolean {
    return !c.display && !c.phoneNumber && !c.email;
  }

  mapFormToApi() {
    const orderNumber = this.form.controls['referenceLoadDisplay'].value;
    const loadStopAndLineItems = this.mapLoadStopsToApi(orderNumber);
    const loadDetailData = {
      loadId: this.form.controls['loadId'].value ? this.form.controls['loadId'].value : '',
      referenceLoadDisplay: orderNumber,
      referenceLoadId: this.form.controls['referenceLoadId'].value ? this.form.controls['referenceLoadId'].value : '',
      commodity: this.form.controls['commodity'].value ? this.form.controls['commodity'].value.commodityName : '',
      equipment: this.selectedEquipment ? this.selectedEquipment.key : '',
      lineItems: loadStopAndLineItems.lineItems,
      contacts: this.mapLoadContactsToApi(this.form.controls['contacts'] as FormArray),
      // If no value, Shipper Pickup Number should default to order number
      shipperPickupNumber: this.form.controls['shipperPickupNumber'].value
        ? this.form.controls['shipperPickupNumber'].value
        : this.form.controls['referenceLoadDisplay'].value,
      transportationMode: this.form.controls['transportationMode'].value ? this.form.controls['transportationMode'].value.name : null,
      services: this.form.controls['services'].value.filter(x => x !== null),
      loadStops: loadStopAndLineItems.loadStops,
      specialInstructions: this.form.controls['specialInstructions'].value,
      miles: 0,
      lineHaulRate: this.form.controls['lineHaulRate'].value ? this.form.controls['lineHaulRate'].value : 0,
      fuelRate: this.form.controls['fuelRate'].value ? this.form.controls['fuelRate'].value : 0,
      weight: 0,
      cube: 0,
    };

    return loadDetailData;
  }

  mapLoadContactsToApi(f: FormArray): LoadContact[] {
    const contacts = [];
    for (let i = 0; i < f.length; i++) {
      const group = f.controls[i] as FormGroup;
      const display = ((group.controls['firstName'].value || '') + ' ' + (group.controls['lastName'].value || '')).trim() || null;
      const contact = {
        display: display,
        phoneNumber: group.controls['phoneNumber'].value,
        email: group.controls['email'].value,
      };

      // The API does not want empty contacts
      if (!this.isApiLoadContactEmpty(contact)) {
        contacts.push(contact);
      }
    }
    return contacts;
  }

  mapLoadStopsToApi(orderNumber: string): { loadStops: LoadStop[]; lineItems: LoadLineItem[] } {
    const loadStops = [];
    let lineItems = [];
    const lineItemNumber = 1;

    for (let i = 0; i < this.loadStops.length; i++) {
      const form = this.loadStops.controls[i] as FormGroup;
      const stop = this.mapLoadStopToApi(form);
      if (stop) {
        loadStops.push(stop);

        if (stop.stopType === StopTypes[StopTypes.Delivery]) {
          const stopLineItems = this.mapLoadLineItemsToApi(form, lineItemNumber, orderNumber);
          if (stopLineItems && stopLineItems.length > 0) {
            lineItems = lineItems.concat(stopLineItems);
          }
        }
      }
    }

    return { loadStops: loadStops, lineItems: lineItems };
  }

  mapLoadStopToApi(f: FormGroup): LoadStop {
    if (f && f.controls) {
      let earlyTime = f.controls['earlyTime'].value ? f.controls['earlyTime'].value : '';
      if (earlyTime) {
        earlyTime = earlyTime.toString().replace(':', '');
      }

      let lateTime = f.controls['lateTime'].value ? f.controls['lateTime'].value : '';
      if (lateTime) {
        lateTime = lateTime.toString().replace(':', '');
      }

      const stop = {
        loadStopId: f.controls['loadStopId'].value,
        loadId: f.controls['loadId'].value,
        stopNbr: f.controls['stopNbr'].value,
        city: f.controls['city'].value,
        state: f.controls['state'].value ? f.controls['state'].value.abbreviation : '',
        country: f.controls['country'].value ? f.controls['country'].value.name : '',
        latitude: f.controls['latitude'].value,
        longitude: f.controls['longitude'].value,
        earlyDtTm: '',
        earlyDate: f.controls['earlyDate'].value ? f.controls['earlyDate'].value : '',
        earlyTime: earlyTime,
        lateDtTm: '',
        lateDate: f.controls['lateDate'].value ? f.controls['lateDate'].value : '',
        lateTime: lateTime,
        apptType: f.controls['apptType'].value,
        instructions: f.controls['instructions'].value,
        appointmentConfirmationCode: f.controls['schedulerConfirmationType'].value
          ? f.controls['schedulerConfirmationType'].value.appointmentConfirmationCode
          : '',
        appointmentSchedulingCode: f.controls['schedulerConfirmationType'].value
          ? f.controls['schedulerConfirmationType'].value.appointmentSchedulingCode
          : '',
        locationName: f.controls['locationName'].value,
        address1: f.controls['address1'].value,
        address2: f.controls['address2'].value,
        address3: f.controls['address3'].value,
        postalCode: f.controls['postalCode'].value,
        isLive: f.controls['isLive'].value,
        stopType: f.controls['stopType'].value,
        contacts: this.mapLoadStopContactsToApi(f.controls['contacts'] as FormArray),
      };

      return stop;
    }
    return null;
  }

  mapLoadStopContactsToApi(f: FormArray): LoadStopContact[] {
    const contacts = [];
    for (let i = 0; i < f.length; i++) {
      const group = f.controls[i] as FormGroup;
      const contact = {
        firstName: group.controls['firstName'].value,
        lastName: group.controls['lastName'].value,
        phoneNumber: group.controls['phoneNumber'].value,
        email: group.controls['email'].value,
      };

      // The API does not want empty contacts
      if (!this.isStopContactEmpty(contact)) {
        contacts.push(contact);
      }
    }
    return contacts;
  }

  mapLoadLineItemsToApi(f: FormGroup, lineItemNumber: number, orderNumber: string): LoadLineItem[] {
    const lineItems = [];
    if (f && f.controls) {
      const formArray = f.controls['lineItems'] as FormArray;
      for (let i = 0; i < formArray.length; i++) {
        const group = formArray.controls[i] as FormGroup;
        const customerPurchaseOrder = group.controls['customerPurchaseOrder'].value;
        if (!customerPurchaseOrder || customerPurchaseOrder.length === 0) {
          group.patchValue({ customerPurchaseOrder: orderNumber });
        }

        const lineItem = {
          loadLineItemNumber: group.controls['loadLineItemNumber'].value || lineItemNumber,
          quantity: group.controls['quantity'].value || 0,
          unitOfMeasure: group.controls['unitOfMeasure'].value,
          weight: group.controls['weight'].value || 0,
          customerPurchaseOrder: group.controls['customerPurchaseOrder'].value,
          pickupStopNumber: group.controls['pickupStopNumber'].value || 1,
          deliveryStopNumber: group.controls['deliveryStopNumber'].value || 0,
        };
        lineItems.push(lineItem);
        lineItemNumber++;
      }
    }
    return lineItems;
  }

  addStop(eventStopIndex: number) {
    const insertIndex = eventStopIndex + 1; // inserting after the index provided
    const stopNumber = insertIndex + 1;
    const lineItem = { ...defaultLoadLineItem, pickupStopNumber: 1, deliveryStopNumber: stopNumber };
    this.loadStops.insert(insertIndex, this.buildLoadStopFormGroup({ ...defaultOrderEntryDeliveryStop, stopNbr: stopNumber }, [lineItem]));

    for (let i = 0; i < this.loadStops.length; i++) {
      this.setStopNbr(this.loadStops.controls[i] as FormGroup, i + 1);
    }

    this.updatePickupStopNumbers();
    this.adjustLineItemPickupStopNumbersForInsertedStop(stopNumber);
  }

  setStopNbr(formGroup: FormGroup, stopNumber: number) {
    formGroup.patchValue({ stopNbr: stopNumber });
    const lineItems = formGroup.get('lineItems') as FormArray;
    for (let i = 0; i < lineItems.length; i++) {
      lineItems.controls[i].patchValue({ deliveryStopNumber: stopNumber });
    }
  }

  adjustLineItemPickupStopNumbersForInsertedStop(insertedStopNumber: number) {
    for (let i = 0; i < this.loadStops.length; i++) {
      const lineItems = this.loadStops.controls[i].get('lineItems') as FormArray;
      for (let j = 0; j < lineItems.length; j++) {
        if (lineItems.controls[j].value.pickupStopNumber >= insertedStopNumber) {
          // if we inserted a stop, increment any pickup stop numbers that are higher than or equal to the insert
          lineItems.controls[j].patchValue({ pickupStopNumber: lineItems.controls[j].value.pickupStopNumber + 1 });
        }
      }
    }
  }

  adjustLineItemPickupStopNumbersForDeletedStop(deletedStopNumber: number) {
    for (let i = 0; i < this.loadStops.length; i++) {
      const lineItems = this.loadStops.controls[i].get('lineItems') as FormArray;
      for (let j = 0; j < lineItems.length; j++) {
        // if we deleted the line items pickup stop, reset it to 1, else if we are after the delete stop decrement
        if (lineItems.controls[j].value.pickupStopNumber === deletedStopNumber) {
          lineItems.controls[j].patchValue({ pickupStopNumber: 1 });
        } else if (lineItems.controls[j].value.pickupStopNumber > deletedStopNumber) {
          lineItems.controls[j].patchValue({ pickupStopNumber: lineItems.controls[j].value.pickupStopNumber - 1 });
        }
      }
    }
  }

  adjustLineItemPickupStopNumbersForChangedStopType(changedStopNumber: number) {
    for (let i = 0; i < this.loadStops.length; i++) {
      const lineItems = this.loadStops.controls[i].get('lineItems') as FormArray;
      for (let j = 0; j < lineItems.length; j++) {
        if (lineItems.controls[j].value.pickupStopNumber === changedStopNumber) {
          lineItems.controls[j].patchValue({ pickupStopNumber: 1 });
        }
      }
    }
  }

  changeStopType(index: number) {
    const changedStopNumber = this.loadStops.value[index].stopNbr;

    this.updatePickupStopNumbers();
    this.adjustLineItemPickupStopNumbersForChangedStopType(changedStopNumber);
  }

  updatePickupStopNumbers() {
    this.pickupStopNumbers = [];
    for (let i = 0; i < this.loadStops.length; i++) {
      const loadStop = this.loadStops.value[i];
      if (loadStop && loadStop.stopType === StopTypes[StopTypes.Pickup]) {
        this.pickupStopNumbers.push({ stopNbr: loadStop.stopNbr });
      }
    }

    this.pickupStopNumbers.sort((a, b) => (a.stopNbr > b.stopNbr ? 1 : -1));
    this.pickupStopNumbers = this.pickupStopNumbers.slice();
  }

  deleteStop(index: number) {
    const removedStop = this.loadStops.controls[index] as FormGroup;
    const removedStopNumber = removedStop.get('stopNbr').value;
    this.loadStops.removeAt(index);

    for (let i = 0; i < this.loadStops.length; i++) {
      this.setStopNbr(this.loadStops.controls[i] as FormGroup, i + 1);
    }

    this.updatePickupStopNumbers();
    this.adjustLineItemPickupStopNumbersForDeletedStop(removedStopNumber);
  }

  cancel() {
    this.router.navigate(['/shipping/home']);
  }

  save() {
    const apiModel = this.mapFormToApi();
    this.apiRequest = JSON.stringify(apiModel, null, 2);

    window.scroll(0, 0);
    if (apiModel) {
      if (apiModel.loadId && apiModel.loadId.length > 0) {
        this.shippingStore.dispatch(new OrderEntryUpdateLoadAction(apiModel));
      } else {
        this.shippingStore.dispatch(new OrderEntryCreateLoadAction(apiModel));
      }
    }
  }

  canDeactivate(): boolean | Observable<boolean> | Promise<boolean> {
    if (!this.form.dirty) {
      return true;
    }

    return new Promise((resolve, reject) => {
      this.confirmationService.confirm({
        message: `Changes will not be saved.  Are you sure you want to proceed?`,
        accept: () => {
          resolve(true);
        },
        reject: () => {
          resolve(false);
        },
      });
    });
  }

  equipmentSelectionMade(node: TreeNode): void {
    if (!node) {
      return;
    }
    this.form.patchValue({
      equipment: { equipmentId: this.selectedEquipment.key, equipmentDesc: this.selectedEquipment.label },
    });
  }
}
