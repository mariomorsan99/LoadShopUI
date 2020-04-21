import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { ReCaptchaV3Service } from 'ng-recaptcha';
import { MenuItem } from 'primeng/api';
import { Observable, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { SecurityAppActionType } from 'src/app/shared/models/security-app-action-type';
import { environment } from '../../../environments/environment';
import {
  AppointmentSchedulerConfirmationType,
  Carrier,
  CarrierCarrierScacGroup,
  Commodity,
  Customer,
  CustomerLoadType,
  Equipment,
  RecaptchaRequest,
  ServiceResponse,
  ServiceType,
  SmartSpotPrice,
  SmartSpotPriceRequest,
  SmartSpotQuoteRequest,
  State,
  TransportationMode,
  UnitOfMeasure,
  UserModel,
} from '../../shared/models';
import { mapResponse } from '../../shared/operators/map-response';
import { CoreState } from '../store';
import { SmartSpotPriceShowQuickQuoteAction } from '../store/actions/smart-spot-price.actions';

@Injectable()
export class CommonService {
  constructor(private http: HttpClient, private store: Store<CoreState>, private recatpchaService: ReCaptchaV3Service) {}

  getMenuItems(user: UserModel): Observable<MenuItem[]> {
    const items: MenuItem[] = [];

    if (!user) {
      return of(items);
    }

    if (!user.hasAgreedToTerms) {
      items.push({ label: 'Terms', routerLink: ['/user-agreement'] });
      return of(items);
    }

    if (user.isCarrier) {
      if (user.hasSecurityAction(SecurityAppActionType.CarrierMarketPlaceView)) {
        items.push({ label: 'Marketplace', routerLink: ['/loads/search'] });
      }
      if (user.hasSecurityAction(SecurityAppActionType.CarrierMyLoadsView)) {
        items.push({ label: 'Booked', routerLink: ['/loads/booked'], badgeStyleClass: 'badge badge-danger' });
      }
      if (user.hasSecurityAction(SecurityAppActionType.CarrierViewDelivered)) {
        items.push({ label: 'Delivered', routerLink: ['/loads/delivered'], badgeStyleClass: 'badge badge-danger' });
      }
    }

    if (user.isShipper) {
      if (user.hasSecurityAction(SecurityAppActionType.ShipperViewActiveLoads)) {
        items.push({ label: 'Post', routerLink: ['/shipping/home'] });
      }
      if (user.hasSecurityAction(SecurityAppActionType.ShipperViewPostedLoads)) {
        items.push({ label: 'Marketplace', routerLink: ['/shipping/marketplace'] });
      }
      if (user.hasSecurityAction(SecurityAppActionType.ShipperViewBookedLoads)) {
        items.push({ label: 'Booked', routerLink: ['/shipping/booked'] });
      }
      if (user.hasSecurityAction(SecurityAppActionType.ShipperViewDeliveredLoads)) {
        items.push({ label: 'Delivered', routerLink: ['/shipping/delivered'] });
      }
      if (user.hasSecurityAction(SecurityAppActionType.ShipperViewSmartSpotPriceQuote)) {
        items.push({
          label: 'Quick Quote',
          command: (event: { originalEvent: Event; item: MenuItem }) => this.store.dispatch(new SmartSpotPriceShowQuickQuoteAction(event)),
        });
      }
    }

    if (user.hasSecurityAction(SecurityAppActionType.AdminTabVisible)) {
      items.push({ label: 'Maintenance', routerLink: ['maint'] });
    }

    return of(items);
  }

  getAdminMenuItems(user: UserModel): Observable<MenuItem[]> {
    const items: MenuItem[] = [];

    if (
      user &&
      (user.hasSecurityAction(SecurityAppActionType.ShipperUserAddEdit) || user.hasSecurityAction(SecurityAppActionType.CarrierUserAddEdit))
    ) {
      items.push({ label: 'Users', routerLink: ['users'] });
    }

    if (user && user.hasSecurityAction(SecurityAppActionType.ShipperAddEdit)) {
      items.push({ label: 'Shipper Profile', routerLink: ['shipper-profile'] });
    }

    if (user && user.hasSecurityAction(SecurityAppActionType.SpecialInstructionsAddEdit)) {
      items.push({ label: 'Special Instructions', routerLink: ['special-instructions'] });
    }

    if (user && user.hasSecurityAction(SecurityAppActionType.ShipperCarrierGroupsAddEdit)) {
      items.push({ label: 'Carrier Groups', routerLink: ['carrier-groups'] });
    }

    if (user && user.hasSecurityAction(SecurityAppActionType.CarrierAddEdit)) {
      items.push({ label: 'Carrier Profile', routerLink: ['carrier-profile'] });
    }
    if (user && user.hasSecurityAction(SecurityAppActionType.UserCommunicationAddEdit)) {
      items.push({ label: 'User Communications', routerLink: ['user-communications'] });
    }

    return of(items);
  }

  getStates(): Observable<State[]> {
    return this.http.get<ServiceResponse<State[]>>(environment.apiUrl + '/api/States/').pipe(mapResponse());
  }

  getEquipment(): Observable<Equipment[]> {
    return this.http.get<ServiceResponse<Equipment[]>>(environment.apiUrl + '/api/Equipment/').pipe(mapResponse());
  }

  getCarriers(): Observable<Carrier[]> {
    return this.http.get<ServiceResponse<Carrier[]>>(environment.apiUrl + '/api/Carrier/').pipe(mapResponse());
  }

  getAllCarrierCarrierScacs(): Observable<CarrierCarrierScacGroup[]> {
    return this.http.get<ServiceResponse<CarrierCarrierScacGroup[]>>(environment.apiUrl + '/api/Carrier/All').pipe(mapResponse());
  }

  getCommodities(): Observable<Commodity[]> {
    return this.http.get<ServiceResponse<Commodity[]>>(environment.apiUrl + '/api/Commodity/').pipe(mapResponse());
  }

  getCustomer(customerId: string): Observable<Customer> {
    return this.http.get<ServiceResponse<Customer>>(environment.apiUrl + `/api/Customer/${customerId}`).pipe(mapResponse());
  }

  getCustomerLoadTypes(): Observable<CustomerLoadType[]> {
    return this.http.get<ServiceResponse<CustomerLoadType[]>>(environment.apiUrl + '/api/CustomerLoadType/').pipe(mapResponse());
  }

  getUnitsOfMeasure(): Observable<UnitOfMeasure[]> {
    return this.http.get<ServiceResponse<UnitOfMeasure[]>>(environment.apiUrl + '/api/UnitOfMeasure/').pipe(mapResponse());
  }

  getTransportationModes(): Observable<TransportationMode[]> {
    return this.http.get<ServiceResponse<TransportationMode[]>>(environment.apiUrl + '/api/TransportationMode/').pipe(mapResponse());
  }

  getServiceTypes(): Observable<ServiceType[]> {
    return this.http.get<ServiceResponse<ServiceType[]>>(environment.apiUrl + '/api/ServiceType/').pipe(mapResponse());
  }

  getAppointmentSchedulerConfirmationTypes(): Observable<AppointmentSchedulerConfirmationType[]> {
    return this.http
      .get<ServiceResponse<AppointmentSchedulerConfirmationType[]>>(environment.apiUrl + '/api/LoadStop/GetAppointmentCodes')
      .pipe(mapResponse());
  }

  getSmartSpotPrice(request: SmartSpotPriceRequest[]): Observable<SmartSpotPrice[]> {
    return this.http.post<ServiceResponse<SmartSpotPrice[]>>(environment.apiUrl + '/api/SmartSpotPrice/', request).pipe(mapResponse());
  }

  getSmartSpotQuote(request: SmartSpotQuoteRequest) {
    // If there is an exception with the resolving the promise, such as Error: Uncaught (in promise): [object Null]
    // check the site key to make sure it's correct for the server
    return this.recatpchaService.execute('SmartSpotPriceQuote').pipe(
      switchMap((token: string) =>
        this.http.post<ServiceResponse<number>>(environment.apiUrl + '/api/SmartSpotPrice/quote', {
          token: token,
          data: request,
        } as RecaptchaRequest<SmartSpotQuoteRequest>)
      ),
      mapResponse()
    );
  }
}
