import { ScrollingModule } from '@angular/cdk/scrolling';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCircle as farCircle, faEdit, faUser } from '@fortawesome/free-regular-svg-icons';
import {
  faArrowDown,
  faArrowLeft,
  faAsterisk,
  faBars,
  faCaretDown,
  faCaretUp,
  faCheckSquare,
  faCircle,
  faEllipsisV,
  faEquals,
  faExclamationCircle,
  faExclamationTriangle,
  faEye,
  faEyeSlash,
  faFileCsv,
  faInfoCircle,
  faMapMarkerAlt,
  faMinus,
  faPencilAlt,
  faPlus,
  faQuestionCircle,
  faSave,
  faSignOutAlt,
  faTimes,
  faTimesCircle,
  faToggleOff,
  faToggleOn,
  faTools,
  faTrash,
  faTrashAlt,
  faUserCircle,
} from '@fortawesome/free-solid-svg-icons';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { TmsSharedModule } from '@tms-ng/shared';
import { ConfirmationService } from 'primeng/api';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { BlockUIModule } from 'primeng/blockui';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { CardModule } from 'primeng/card';
import { CarouselModule } from 'primeng/carousel';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/components/common/messageservice';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { EditorModule } from 'primeng/editor';
import { FieldsetModule } from 'primeng/fieldset';
import { FileUploadModule } from 'primeng/fileupload';
import { GrowlModule } from 'primeng/growl';
import { InputMaskModule } from 'primeng/inputmask';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessagesModule } from 'primeng/messages';
import { MultiSelectModule } from 'primeng/multiselect';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { PaginatorModule } from 'primeng/paginator';
import { PanelModule } from 'primeng/panel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { RadioButtonModule } from 'primeng/radiobutton';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { SelectButtonModule } from 'primeng/selectbutton';
import { SidebarModule } from 'primeng/sidebar';
import { SliderModule } from 'primeng/slider';
import { TableModule } from 'primeng/table';
import { TabMenuModule } from 'primeng/tabmenu';
import { TabViewModule } from 'primeng/tabview';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { TooltipModule } from 'primeng/tooltip';
import { TreeModule } from 'primeng/tree';
import { VirtualScrollerModule } from 'primeng/virtualscroller';
import { DataViewModule } from 'primeng/dataview';

import {
  ActionCheckDirective,
  AutoFocusDirective,
  CloseDropdownOnScrollDirective,
  CloseMultiSelectOnScrollDirective,
  CurrencyInputDirective,
  StopClickPropagationDirective,
} from '../shared/directives';
import {
  AddressAutocompleteComponent,
  ColorBarComponent,
  ContactFormComponent,
  CustomerTypeIconComponent,
  DaysComponent,
  DropdownComponent,
  LoadDetailComponent,
  LoadDetailContainerComponent,
  LoadLineItemFormComponent,
  LoadStopComponent,
  LoadStopFormComponent,
  MapComponent,
  SidebarCloseComponent,
  TreeDropDownComponent,
  TripDisplayComponent,
  UploadDocumentModalComponent,
  UserCommunicationDisplayComponent,
  FeedbackComponent,
} from './components';
import { AppActionGuard, RootGuard } from './guards';
import { DayAbbreviationPipe, ToSelectItemByKeyPipe, ToSelectItemPipe, ToTreeNodesPipe } from './pipes';
import { LoadDocumentService, UserCommunicationDisplayService } from './services';
import { effects, reducers } from './store';
import { FeedbackService } from './services/feedback.service';
import { PercentageComponent } from './components/percentage/percentage.component';

library.add(faTrash);
library.add(faBars);
library.add(faTimes);
library.add(faTimesCircle);
library.add(faTrashAlt);
library.add(faSave);
library.add(faPlus);
library.add(faArrowDown);
library.add(faEye);
library.add(faEyeSlash);
library.add(faToggleOn);
library.add(faToggleOff);
library.add(faPencilAlt);
library.add(faQuestionCircle);
library.add(faExclamationCircle);
library.add(faExclamationTriangle);
library.add(faInfoCircle);
library.add(faUserCircle);
library.add(faSignOutAlt);
library.add(faFileCsv);
library.add(faCircle);
library.add(faUser);
library.add(faArrowLeft);
library.add(faCaretUp);
library.add(faCaretDown);
library.add(farCircle);
library.add(faMapMarkerAlt);
library.add(faEllipsisV);
library.add(faEdit);
library.add(faMinus);
library.add(faEquals);
library.add(faAsterisk);
library.add(faTools);
library.add(faCheckSquare);

const primeModules = [
  PanelModule,
  InputMaskModule,
  CheckboxModule,
  ButtonModule,
  TableModule,
  SelectButtonModule,
  DialogModule,
  AutoCompleteModule,
  CardModule,
  InputSwitchModule,
  ToggleButtonModule,
  TabViewModule,
  DropdownModule,
  MessagesModule,
  CalendarModule,
  SidebarModule,
  MultiSelectModule,
  InputTextareaModule,
  PaginatorModule,
  GrowlModule,
  TabMenuModule,
  ConfirmDialogModule,
  CarouselModule,
  TooltipModule,
  SliderModule,
  VirtualScrollerModule,
  BlockUIModule,
  ProgressSpinnerModule,
  ScrollPanelModule,
  EditorModule,
  OverlayPanelModule,
  FieldsetModule,
  TreeModule,
  RadioButtonModule,
  FileUploadModule,
  DataViewModule
];

const directives = [
  AutoFocusDirective,
  StopClickPropagationDirective,
  TripDisplayComponent,
  CurrencyInputDirective,
  CloseMultiSelectOnScrollDirective,
  CloseDropdownOnScrollDirective,
  ActionCheckDirective,
];

@NgModule({
  declarations: [
    AddressAutocompleteComponent,
    ColorBarComponent,
    DaysComponent,
    DayAbbreviationPipe,
    ToSelectItemPipe,
    ToSelectItemByKeyPipe,
    ToTreeNodesPipe,
    MapComponent,
    SidebarCloseComponent,
    ContactFormComponent,
    CustomerTypeIconComponent,
    LoadLineItemFormComponent,
    LoadStopFormComponent,
    TreeDropDownComponent,
    DropdownComponent,
    LoadDetailComponent,
    LoadDetailContainerComponent,
    LoadStopComponent,
    UserCommunicationDisplayComponent,
    UploadDocumentModalComponent,
    FeedbackComponent,
    PercentageComponent,
    ...directives,
  ],
  imports: [
    TmsSharedModule,
    CommonModule,
    FormsModule,
    ...primeModules,
    FontAwesomeModule,
    ReactiveFormsModule,
    ScrollingModule,
    EffectsModule.forFeature(effects),
    StoreModule.forFeature('sharedstate', reducers),
  ],
  providers: [
    MessageService,
    ConfirmationService,
    AppActionGuard,
    RootGuard,
    UserCommunicationDisplayService,
    LoadDocumentService,
    FeedbackService],
  exports: [
    AddressAutocompleteComponent,
    ColorBarComponent,
    DaysComponent,
    DayAbbreviationPipe,
    ToSelectItemPipe,
    ToSelectItemByKeyPipe,
    ToTreeNodesPipe,
    MapComponent,
    TmsSharedModule,
    CommonModule,
    FormsModule,
    ...primeModules,
    FontAwesomeModule,
    ReactiveFormsModule,
    ScrollingModule,
    SidebarCloseComponent,
    TripDisplayComponent,
    ContactFormComponent,
    CustomerTypeIconComponent,
    LoadLineItemFormComponent,
    LoadStopFormComponent,
    TreeDropDownComponent,
    DropdownComponent,
    LoadDetailContainerComponent,
    UserCommunicationDisplayComponent,
    UploadDocumentModalComponent,
    FeedbackComponent,
    PercentageComponent,
    ...directives,
  ],
})
export class SharedModule {}
