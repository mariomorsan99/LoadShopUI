import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { select, Store } from '@ngrx/store';
import { ConfirmationService } from 'primeng/api';
import { Observable, Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';
import { CoreState, getEquipment, getStates } from 'src/app/core/store';
import { AllMessageTypes, Equipment, State, User, UserLane } from 'src/app/shared/models';
import {
  getUserLaneSelectedLane,
  getUserProfileEntity,
  UserLaneAddAction,
  UserLaneDeleteAction,
  UserLaneUpdateAction,
  UserState,
} from '../../store';

@Component({
  selector: 'kbxl-user-lane-detail-container',
  templateUrl: './user-lane-detail-container.component.html',
  styleUrls: ['./user-lane-detail-container.component.css'],
})
export class UserLaneDetailContainerComponent implements OnInit, OnDestroy {
  states$: Observable<State[]>;
  equipment$: Observable<Equipment[]>;
  laneDetail$: Observable<UserLane>;
  displaySidebar: boolean;

  destroyed$ = new Subject<boolean>();
  private currentUser: User;

  constructor(
    private store: Store<UserState>,
    private coreStore: Store<CoreState>,
    private route: ActivatedRoute,
    private router: Router,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.states$ = this.coreStore.pipe(map(getStates));
    this.equipment$ = this.coreStore.pipe(map(getEquipment));
    this.laneDetail$ = this.store.pipe(map(getUserLaneSelectedLane));
    this.store.pipe(map(getUserLaneSelectedLane), takeUntil(this.destroyed$)).subscribe(detail => {
      if (detail == null) {
        // on page refresh we will not have a lane detail so we should close
        setTimeout(() => this.onHide());
      }
    });

    this.store.pipe(select(getUserProfileEntity), takeUntil(this.destroyed$)).subscribe(user => {
      if (!user) {
        return;
      }
      this.currentUser = user;
    });
  }

  ngOnDestroy() {
    this.cleanupSubscription();
  }

  private cleanupSubscription() {
    this.destroyed$.next(true);
    this.destroyed$.complete();
  }

  onHide() {
    this.cleanupSubscription();

    // navigate up two segments from our detail/:id path
    this.router.navigate(['../..'], { relativeTo: this.route });
  }

  updated(lane: UserLane) {
    if (lane.origCity) {
      if (!lane.origDH) {
        lane.origDH = 50;
      }
    } else {
      lane.origDH = null;
    }

    if (lane.destCity) {
      if (!lane.destDH) {
        lane.destDH = 50;
      }
    } else {
      lane.destDH = null;
    }

    if (!lane.userLaneId) {
      this.store.dispatch(new UserLaneAddAction(lane));
    } else {
      this.store.dispatch(new UserLaneUpdateAction(lane));
    }

    if (this.currentUser) {
      const notificationsEnabled = lane.userLaneMessageTypes.some(x => x.selected);

      if (notificationsEnabled && !this.currentUser.isNotificationsEnabled) {
        this.confirmationService.confirm({
          rejectVisible: false,
          acceptLabel: 'OK',
          message: `You have selected to receive notifications, but have them disabled on your profile. Please update.`,
        });
      } else {
        // check to make sure sms is enabled
        const smsEnabled = lane.userLaneMessageTypes.find(x => x.messageTypeId === AllMessageTypes.CellPhone);

        const userProfileSmsNotifications = this.currentUser.userNotifications.filter(x => x.messageTypeId === AllMessageTypes.CellPhone);

        const userProfileSmsEnabled = userProfileSmsNotifications.some(x => x.notificationEnabled);
        if (smsEnabled && smsEnabled.selected && !userProfileSmsEnabled) {
          this.confirmationService.confirm({
            rejectVisible: false,
            acceptLabel: 'OK',
            message: `You have selected to receive SMS notifications,
            but have them disabled on your profile.
            Please select at least 1 Cell Phone to receive notifications.`,
          });
        }
      }
    }

    this.onHide();
  }

  deleted(lane: UserLane) {
    this.store.dispatch(new UserLaneDeleteAction(lane));
    this.onHide();
  }

  closed(lane: UserLane) {
    this.onHide();
  }
}
