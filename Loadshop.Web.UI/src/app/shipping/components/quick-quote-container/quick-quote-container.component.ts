import { Component, OnInit, ViewChild } from '@angular/core';
import { OverlayPanel } from 'primeng/overlaypanel';
import { Store, select } from '@ngrx/store';
import {
  CoreState,
  getSmartSpotQuickQuoteEvent,
  SmartSpotPriceHideQuickQuoteAction,
  getStates,
  getEquipment,
  getSmartSpotLoadingQuote,
  getSmartSpotQuickQuoteValue,
  SmartSpotPriceLoadQuoteAction,
  SmartSpotPriceLoadQuoteSuccessAction,
  getSmartSpotQuickQuoteErrors,
  SmartSpotCreateOrderFromQuoteAction
} from 'src/app/core/store';
import { Observable } from 'rxjs';
import {
  State,
  Equipment,
  SmartSpotQuoteRequest,
  ValidationProblemDetails,
  SmartSpotQuoteCreateRequest,
  User
} from 'src/app/shared/models';
import { getUserProfileEntity, UserState } from 'src/app/user/store';

@Component({
  selector: 'kbxl-quick-quote-container',
  templateUrl: './quick-quote-container.component.html',
  styleUrls: ['./quick-quote-container.component.scss']
})
export class QuickQuoteContainerComponent implements OnInit {
  @ViewChild(OverlayPanel) quickQuotePanel: OverlayPanel;

  public states$: Observable<State[]>;
  public equipment$: Observable<Equipment[]>;
  public loading$: Observable<boolean>;
  public quoteValue$: Observable<number>;
  public quoteErrors$: Observable<ValidationProblemDetails>;
  public userProfile$: Observable<User>;

  constructor(private store: Store<CoreState>, private userStore: Store<UserState>) { }

  ngOnInit() {
    this.states$ = this.store.pipe(select(getStates));
    this.equipment$ = this.store.pipe(select(getEquipment));
    this.loading$ = this.store.pipe(select(getSmartSpotLoadingQuote));
    this.quoteValue$ = this.store.pipe(select(getSmartSpotQuickQuoteValue));
    this.quoteErrors$ = this.store.pipe(select(getSmartSpotQuickQuoteErrors));
    this.userProfile$ = this.userStore.pipe(select(getUserProfileEntity));

    this.store.pipe(select(getSmartSpotQuickQuoteEvent))
    .subscribe(_ => {
      if (_ && _.originalEvent) {
        if (!this.quickQuotePanel.visible) {
          this.quickQuotePanel.show(_.originalEvent);
        }
      } else if (this.quickQuotePanel.visible) {
        this.quickQuotePanel.hide();
      }
    });
  }

  onHide() {
    this.store.dispatch(new SmartSpotPriceHideQuickQuoteAction());
  }

  clear() {
    this.store.dispatch(new SmartSpotPriceLoadQuoteSuccessAction(null));
  }

  getQuote(request: SmartSpotQuoteRequest) {
    this.store.dispatch(new SmartSpotPriceLoadQuoteAction(request));
  }

  createOrder(request: SmartSpotQuoteCreateRequest) {
    this.store.dispatch(new SmartSpotCreateOrderFromQuoteAction(request));
  }
}
