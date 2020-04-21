import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService } from 'primeng/api';
import { Observable, Subject } from 'rxjs';
import { delay, tap } from 'rxjs/operators';
import { CanComponentDeactivate, CanDeactivateGuard } from '../../../core/guards/can-deactivate.guard';
// import { getStates } from '../../store';
import { CoreState, getCommodities, getEquipment, getLoadingCommodities, getStates } from '../../../core/store';
import {
  Commodity,
  Equipment,
  LoadStatusNotificationsData,
  State,
  User,
  UserLane,
  UserModel,
  ValidationProblemDetails,
} from '../../../shared/models';
import { SecurityAppActionType } from '../../../shared/models/security-app-action-type';
import { UserProfileService } from '../../services';
import {
  getLoadStatusNotifications,
  getLoadStatusNotificationsProblemDetails,
  getSavedLoadStatusNotificationsProblemDetails,
  getUserLaneEntities,
  getUserLaneLoading,
  getUserProfileEntity,
  getUserProfileLoading,
  getUserProfileProblemDetails,
  LoadStatusNotificationsLoadAction,
  LoadStatusNotificationsUpdateAction,
  UserLaneAddAction,
  UserLaneDeleteAction,
  UserLaneSelectedDetailAction,
  UserLaneUpdateAction,
  UserProfileLoadAction,
  UserProfileUpdateAction,
  UserState,
} from '../../store';

@Component({
  templateUrl: './user-profile-container.component.html',
  styleUrls: ['./user-profile-container.component.scss'],
})
export class UserProfileContainerComponent implements OnInit, CanDeactivateGuard {
  userProfile$: Observable<User>;
  userLanes$: Observable<UserLane[]>;
  states$: Observable<State[]>;
  equipment$: Observable<Equipment[]>;
  commodities$: Observable<Commodity[]>;
  loadingCommodities$: Observable<boolean>;
  loadingProfile$: Observable<boolean>;
  loadingLanes$: Observable<boolean>;
  savedLoadStatusNotifications$: Observable<boolean>;
  error$: Observable<ValidationProblemDetails>;
  loadStatusNotificationsErrors$: Observable<ValidationProblemDetails>;
  loadStatusNotifications$: Observable<LoadStatusNotificationsData>;
  profileUpdatedModalShown = false;
  triggerValidation: Subject<boolean> = new Subject();
  private currentUser: User;
  constructor(
    private store: Store<UserState>,
    private coreStore: Store<CoreState>,
    private router: Router,
    private route: ActivatedRoute,
    private confirmationService: ConfirmationService,
    private userProfileService: UserProfileService
  ) {}

  ngOnInit() {
    this.store.dispatch(new UserProfileLoadAction());

    this.userProfile$ = this.store.pipe(
      select(getUserProfileEntity),
      delay(0),
      tap(user => {
        if (!user) {
          return;
        }
        this.currentUser = user;

        if (!this.userProfileService.validateUserNotifications(user)) {
          this.showProfileNeedsUpdateModal();
        }
        // LoadStatusNotifications
        const userModel = new UserModel(user);
        if (userModel.hasSecurityAction(SecurityAppActionType.LoadStatusNotifications)) {
          this.store.dispatch(new LoadStatusNotificationsLoadAction());
        }
      })
    );
    this.userLanes$ = this.store.pipe(select(getUserLaneEntities));
    this.states$ = this.coreStore.pipe(select(getStates));
    this.equipment$ = this.coreStore.pipe(select(getEquipment));
    this.loadingProfile$ = this.coreStore.pipe(select(getUserProfileLoading));
    this.loadingLanes$ = this.coreStore.pipe(select(getUserLaneLoading));
    this.commodities$ = this.coreStore.pipe(select(getCommodities));
    this.loadingCommodities$ = this.coreStore.pipe(select(getLoadingCommodities));
    this.loadStatusNotifications$ = this.store.pipe(select(getLoadStatusNotifications));
    this.loadStatusNotificationsErrors$ = this.store.pipe(select(getLoadStatusNotificationsProblemDetails));
    this.savedLoadStatusNotifications$ = this.store.pipe(select(getSavedLoadStatusNotificationsProblemDetails));
    this.error$ = this.coreStore.pipe(select(getUserProfileProblemDetails));
  }

  canDeactivate(component: CanComponentDeactivate): boolean | Observable<boolean> | Promise<boolean> {
    if (this.currentUser && !this.userProfileService.validateUserNotifications(this.currentUser)) {
      this.triggerValidation.next(true);
      return false;
    }
    return true;
  }

  displayDetail(lane: UserLane) {
    this.store.dispatch(new UserLaneSelectedDetailAction(lane));
    this.router.navigate(['lane/details'], { relativeTo: this.route });
  }

  added(lane: UserLane) {
    this.store.dispatch(new UserLaneAddAction(lane));
  }

  updated(lane: UserLane) {
    this.store.dispatch(new UserLaneUpdateAction(lane));
  }

  deleted(lane: UserLane) {
    this.store.dispatch(new UserLaneDeleteAction(lane));
  }

  updateProfile(user: User) {
    this.currentUser = user;
    this.store.dispatch(new UserProfileUpdateAction(user));
  }

  showProfileNeedsUpdateModal(): void {
    if (this.profileUpdatedModalShown) {
      return;
    }
    // this.triggerValidation.next(true);
    this.profileUpdatedModalShown = true;
    this.confirmationService.confirm({
      rejectVisible: false,
      acceptLabel: 'OK',
      message: `Please update phone and email address`,
    });
  }
  onSaveLoadStatusNotifications(payload: LoadStatusNotificationsData): void {
    this.store.dispatch(new LoadStatusNotificationsUpdateAction(payload));
  }
}
