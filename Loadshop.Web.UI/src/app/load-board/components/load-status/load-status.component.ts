import { Component, Input, ChangeDetectionStrategy, Output, EventEmitter, OnDestroy } from '@angular/core';
import { LoadStatusDetail, LoadStatusInTransitData, ValidationProblemDetails, UserModel, TransactionType } from 'src/app/shared/models';
import {
  LoadDetail,
  LoadStatusTypes,
  LoadStop,
  Place,
  State,
  LoadStatusStopData,
  LoadStatusStopEventData,
  StopEventTypes
} from 'src/app/shared/models';
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import * as moment from 'moment';
import { SecurityAppActionType } from 'src/app/shared/models/security-app-action-type';

@Component({
  selector: 'kbxl-load-status',
  templateUrl: './load-status.component.html',
  styleUrls: ['./load-status.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadStatusComponent implements OnDestroy {
  private _loadDetail: LoadDetail;
  @Input() set loadDetail(value: LoadDetail) {
    this._loadDetail = value;
    this.buildStopForm();
    this.buildInTransitForm();
  }
  get loadDetail() {
    return this._loadDetail;
  }

  private _loadStatus: LoadStatusDetail;
  @Input() set loadStatus(value: LoadStatusDetail) {
    this._loadStatus = value;
    this.buildStopForm();
    this.buildInTransitForm();
  }
  get loadStatus() {
    return this._loadStatus;
  }

  @Input() set loadStatusErrors(value: ValidationProblemDetails) {
    this.setErrors(value ? value.errors || {} : {});
  }

  @Input() loadingStatus: boolean;
  @Input() savingStatus: boolean;
  @Input() states: State[];
  @Input() user: UserModel;

  @Output() saveStopStatuses = new EventEmitter<LoadStatusStopData>();
  @Output() saveInTransitStatus = new EventEmitter<LoadStatusInTransitData>();

  get accepted() {
    return this._loadDetail && this._loadDetail.loadTransaction
          && this._loadDetail.loadTransaction.transactionType === TransactionType.Accepted;
  }

  private _stopPanelCollapsed = false;
  public get stopPanelCollapsed() {
    return this._stopPanelCollapsed;
  }
  public set stopPanelCollapsed(value: boolean) {
    this._stopPanelCollapsed = value;
  }
  public get inTransitPanelCollapsed() {
    return !this._stopPanelCollapsed;
  }
  public set inTransitPanelCollapsed(value: boolean) {
    this._stopPanelCollapsed = !value;
  }

  public get maxDate() {
    return new Date();
  }

  public get displayStatusForms() {
    return !this.loadingStatus && this.loadStatus
        && !this.loadStatus.processingUpdates && this.loadStatus.status !== LoadStatusTypes.Delivered
        && this.user && this.user.hasSecurityAction(SecurityAppActionType.CarrierUpdateStatus)
        && this.accepted;
  }

  public loadStatusTypes = LoadStatusTypes;
  public stopForm: FormGroup;
  public inTransitForm: FormGroup;
  public suggestions: Place[];

  public inTransitErrorSummary: string;
  public inTransitErrorCount: number;
  public stopStatusesErrorSummary: string;
  public stopStatusesErrorCount: number;

  private stopFormValueChangesSub: Subscription;

  public canSubmitInTransit = false;
  public availableInTransitStops: LoadStop[] = [];

  private inTransitMap = [
    { urn: '', formControlName: '' },
    { urn: 'LocationTime', formControlName: 'statusTime' },
    { urn: 'Latitude', formControlName: 'location' },
    { urn: 'Longitude', formControlName: 'location' },
  ];

  private stopStatusTimeMap = [
    { urn: '', formControlName: '' },
    { urn: 'EventTime', formControlName: 'statusTime' },
  ];


  public constructor(private fb: FormBuilder) {
    this.stopForm = this.fb.group({ stops: this.fb.array([]) });
    this.inTransitForm = this.fb.group({
      stopNumber: null,
      location: null,
      statusTime: null,
      timeInput: null
    });
  }

  ngOnDestroy() {
    this.stopFormValueChangesSub.unsubscribe();
  }

  private getAvailableStops() {
    if (this.accepted && this.loadStatus) {
      return this.loadDetail.loadStops.filter(stop => {
        return stop.stopNbr > this.loadStatus.stopNumber
          || (stop.stopNbr === this.loadStatus.stopNumber && this.loadStatus.status !== LoadStatusTypes.Departed);
      });
    }
    return [];
  }

  private getAvailableInTransitStops() {
    if (this.accepted && this.loadStatus
        && (this.loadStatus.status === LoadStatusTypes.Departed
            || this.loadStatus.status === LoadStatusTypes.InTransit)) {
      return this.loadDetail.loadStops.filter(stop => {
        return stop.stopNbr > this.loadStatus.stopNumber
          || (stop.stopNbr === this.loadStatus.stopNumber && this.loadStatus.status === LoadStatusTypes.InTransit);
      });
    }
    return [];
  }

  private buildStopForm() {
    if (this.stopFormValueChangesSub) {
      this.stopFormValueChangesSub.unsubscribe();
      this.stopFormValueChangesSub = null;
    }

    const stopGroups = this.getAvailableStops().map(_ => this.buildStopGroup(_));
    this.stopForm = this.fb.group({ stops: this.fb.array(stopGroups) });
    this.updateEnabledTimes();

    this.stopFormValueChangesSub = this.stopForm.valueChanges.subscribe(() => {
      this.updateEnabledTimes();
    });
  }

  private buildStopGroup(stop: LoadStop) {
    const statusTimes = [];

    const isFirstStop = this.loadDetail.loadStops[0] === stop;
    const isLastStop = this.loadDetail.loadStops[this.loadDetail.loadStops.length - 1] === stop;

    if (stop.stopNbr > this.loadStatus.stopNumber
        || (stop.stopNbr === this.loadStatus.stopNumber
            && this.loadStatus.status === LoadStatusTypes.InTransit)) {
      statusTimes.push(this.fb.group({
        status: LoadStatusTypes.Arrived,
        statusTime: new FormControl({ value: null, disabled: !isFirstStop }),
        timeInput: new FormControl({ value: null, disabled: !isFirstStop })
      }));
    }
    if (stop.stopNbr > this.loadStatus.stopNumber
        || (stop.stopNbr === this.loadStatus.stopNumber
            && this.loadStatus.status !== LoadStatusTypes.Departed)) {
      statusTimes.push(this.fb.group({
        status: isLastStop ? LoadStatusTypes.Delivered : LoadStatusTypes.Departed,
        statusTime: new FormControl({ value: null, disabled: !isFirstStop || statusTimes.length > 0 }),
        timeInput: new FormControl({ value: null, disabled: !isFirstStop || statusTimes.length > 0 })
      }));
    }

    return this.fb.group({
      stopTitle: isFirstStop ? 'Pickup Stop' : isLastStop ? 'Final Stop' : `Stop ${stop.stopNbr}`,
      stopNumber: stop.stopNbr,
      statusTimes: this.fb.array(statusTimes)
    });
  }

  private updateEnabledTimes() {
    let enabled = true;
    (this.stopForm.controls['stops'] as FormArray).controls.forEach((stop: FormGroup) => {
      (stop.controls['statusTimes'] as FormArray).controls.forEach((statusTime: FormGroup) => {
        if (enabled) {
          statusTime.enable({ emitEvent: false });
        } else {
          statusTime.disable({ emitEvent: false });
          statusTime.patchValue({ statusTime: null }, { emitEvent: false });
          statusTime.patchValue({ timeInput: null }, { emitEvent: false });
        }
        if (statusTime.value.statusTime == null) {
          enabled = false;
        }
      });
    });
  }

  private buildInTransitForm() {
    this.availableInTransitStops = this.getAvailableInTransitStops();
    if (this.availableInTransitStops.length > 0) {
      this.inTransitForm.patchValue({
        stopNumber: this.availableInTransitStops[0].stopNbr,
        location: null,
        statusTime: null,
        timeInput: null
      });
    }
  }

  public submitStopStatuses() {
    const stopEvents = [];
    const stops = this.stopForm.value.stops;

    let eventCounter = 0;
    for (let i = 0; i < stops.length; i++) {
      const stop = stops[i];
      for (let j = 0; j < (stop.statusTimes || []).length; j++) {
        const status = stop.statusTimes[j];
        if (status.statusTime != null) {
          const eventType = status.status === LoadStatusTypes.Arrived ? StopEventTypes.Arrival
                          : status.status === LoadStatusTypes.Departed ? StopEventTypes.Departure
                          : status.status === LoadStatusTypes.Delivered ? StopEventTypes.Actual
                          : null;

          const statusDate = (status.statusTime as Date);
          statusDate.setHours(0, 0, 0);

          if (status.timeInput) {
            const timeParts = status.timeInput.split(':');
            if (timeParts.length === 2) {
              const hours = parseInt(timeParts[0], 10),
                    minutes = parseInt(timeParts[1], 10);
              if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) {
                const error = {};
                error[`urn:root:Events:${eventCounter}:EventTime`] = ['Time is invalid'];
                this.setErrors(error);
                return;
              }

              statusDate.setHours(hours, minutes, 0);
            }
          }

          stopEvents.push({
            stopNumber: stop.stopNumber,
            eventType: eventType,
            eventTime: moment(statusDate).milliseconds(0).seconds(0).format('YYYY-MM-DDTHH:mm:ss'),
          } as LoadStatusStopEventData);
        }

        eventCounter++;
      }
    }

    const stopStatuses = {
      loadId: this._loadDetail.loadId,
      events: stopEvents
    } as LoadStatusStopData;

    this.saveStopStatuses.emit(stopStatuses);
  }

  public submitInTransit() {
    let statusDate = null;
    if (this.inTransitForm.value.statusTime) {
      statusDate = (this.inTransitForm.value.statusTime as Date);
      statusDate.setHours(0, 0, 0);

      if (this.inTransitForm.value.timeInput) {
        const timeParts = this.inTransitForm.value.timeInput.split(':');
        if (timeParts.length === 2) {
          const hours = parseInt(timeParts[0], 10),
                minutes = parseInt(timeParts[1], 10);
          if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) {
            this.setErrors({
              'urn:root:LocationTime': ['Time is invalid']
            });
            return;
          }

          statusDate.setHours(hours, minutes, 0);
        }
      }
    }

    const inTransitStatus = {
      loadId: this._loadDetail.loadId,
      locationTime: statusDate
        ? moment(statusDate).milliseconds(0).seconds(0).format('YYYY-MM-DDTHH:mm:ss')
                    : null,
      latitude: this.inTransitForm.value.location ? this.inTransitForm.value.location.latitude : null,
      longitude: this.inTransitForm.value.location ? this.inTransitForm.value.location.longitude : null,
    } as LoadStatusInTransitData;

    this.saveInTransitStatus.emit(inTransitStatus);
  }

  private setErrors(errors: object) {
    const urnRoot = 'urn:root';

    const messages = this.setFormGroupErrors(this.inTransitForm, urnRoot, this.inTransitMap, errors);
    this.inTransitErrorSummary = messages ? messages.join('\n') : '';
    this.inTransitErrorCount = messages ? messages.length : 0;

    this.setStopErrors(urnRoot, errors);
  }

  private setFormGroupErrors(
      formObject: FormGroup | FormArray,
      urnPrefix: string, errorMap: { urn: string; formControlName: string }[], errors
      ): string[] {
    const errorList: string[] = [];
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
            errorList.push(error.trim());
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
    return errorList;
  }

  private setStopErrors(urnRoot: string, errors: object) {
    let eventIndex = 0;
    const stops = this.stopForm.controls['stops'] as FormArray;

    let messages: string[] = [];

    for (let i = 0; i < stops.length; i++) {
      const stopGroup = stops.controls[i] as FormGroup;
      const statusTimes = stopGroup.controls['statusTimes'] as FormArray;
      for (let j = 0; j < statusTimes.length; j++) {
        const eventUrnPrefix = `${urnRoot}:Events:${eventIndex}`;
        const groupMessages = this.setFormGroupErrors(statusTimes.controls[j] as FormGroup, eventUrnPrefix, this.stopStatusTimeMap, errors);
        messages = messages.concat(groupMessages);
        eventIndex++;
      }
    }

    this.stopStatusesErrorSummary = messages ? messages.join('\n') : '';
    this.stopStatusesErrorCount = messages ? messages.length : 0;
  }

  sendStopOnCalendarFocus(event, stopIndex, timeIndex) {
    const stops = this.stopForm.controls['stops'] as FormArray;

    if (stops && stops.length >= stopIndex) {
      const stopGroup = stops.controls[stopIndex] as FormGroup;
      const statusTimes = stopGroup.controls['statusTimes'] as FormArray;

      if (statusTimes && statusTimes.length >= timeIndex) {
        const statusTime = statusTimes.controls[timeIndex] as FormGroup;
        if (statusTime.value.statusTime == null) {
          statusTime.patchValue({ statusTime: new Date() }, { emitEvent: false });
          this.updateEnabledTimes();
        }
      }
    }
  }

  inTransitOnCalendarFocus() {
    const form = this.inTransitForm as FormGroup;
    if (form.value.statusTime == null) {
      form.patchValue({ statusTime: new Date() }, { emitEvent: false });
    }
  }
}
