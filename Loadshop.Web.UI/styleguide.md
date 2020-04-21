# Loadshop Style Guide

## Overview

This style guide is not all inclusive and is meant for a starting point to help consolidate and direct future development into some common practices and standards.

## Dependencies and 3rd Party Libraries

Currently, we utilize the following libraries:

- [PrimeNG](https://primefaces.org/primeng/#/) - Components, CSS classes

- [Bootstrap](https://getbootstrap.com/docs/4.4/getting-started/introduction/) - Primarily used for grid system, text, alignment, and coloring classes

- [Font Awesome](https://fontawesome.com/) - Icons

Please refer to their style guides as this will build upon that.

## SASS - Syntactically Awesome Style Sheets

Loadshop utilizes SASS files for library dependencies and Angular components. The `.angular.json` contains a reference to the **./src/styles/** directory so all files can be referenced in component scss files using the following syntax vs relative pathing:

```
@import 'variables';
```

### Hierarchy / Load Order

The SASS files are broken out into several files and are organized like the following:

```
Variables.scss
|-> bootstrap-overrides.scss
    |-> Bootstrap SASS files
        |-> Angular components
|-> prime-ng-overrides.scss
|-> Angular Components

> styles.scss
```

#### Variables.scss

The `variables.scss` contains all Loadshop colors, spacing, font, etc variables. This file is referenced by Angular components and override files also.

#### Bootstrap Themeing / overrides

The `bootstrap-overrides.scss` contains all overrides for bootstrap variables and then imports all the bootstrap components (SCSS) needed for the project. Will also contain any specific bootstrap overrides that cannot be handled via variables

#### Prime NG Overrides

The `prime-ng-overrides.scss` contains all overrides for the PrimeNG controls. Note that these styles are overriding default PrimeNG themes rather than rebuilding them like Bootstrap

#### Styles.scss

The `styles.scss` is the main style file and loaded last and will apply all styles globally. Font Awesome, PrimeNG, Bootstrap are all imported in this file first; followed by all user defined custom classes.

## Layouts

### Bootstrap Grid

For general / simple layouts, use the Bootstrap Grid system. More info [here](https://getbootstrap.com/docs/4.4/layout/overview/)

#### Media Queries

If your component needs specific styling for smaller / larger size screens, reference the Bootstrap overrides file and use the media queries:

```
@import 'bootstrap-overrides';

@include media-breakpoint-up(sm) { ... }
@include media-breakpoint-up(md) { ... }
@include media-breakpoint-up(lg) { ... }
@include media-breakpoint-up(xl) { ... }
```

### Bootstrap Flex Utility Classes

For specific layouts in components, you should lean towards using the Boostrap Flex Utilitie classes found [here](https://getbootstrap.com/docs/4.4/utilities/flex/)

## Components

Other components not listed below use default Prime NG implementations.

- `<p-dropdown>`
- `<p-checkbox>`
- `<p-checkbox>`
- `<p-tabMenu>`
- `<p-panel>`
- `<p-multiSelect>`
- `<p-table>`
- `<p-radioButton>`
- `<p-inputSwitch>`
- `<p-autoComplete>`
- `<p-editor>`
- `<p-calendar>`
- `<p-slider>`
- `<p-dialog>`

> For values such as Carriers, Names, Origin / Destination, when bound into a component be sure to use the template to ensure that it is properly cased using the `titlecase` pipe, example:

```
<p-multiSelect
    defaultLabel="Select"
    [options]="availableCarriers"
  >
    <ng-template let-item pTemplate="item">
      <span>{{ item?.label | titlecase }}</span>
    </ng-template>
  </p-multiSelect>
```

### Buttons

All buttons are currently PrimeNG buttons `pButton`:

```
 <button pButton
        label="Save"
        class="ui-button-primary"
        ></button>
```

#### Alignments

Layouts of buttons should aligned to the right side of the page / container and should be vertically aligned with any other buttons on the page

#### Primary, Secondary, Danger, Teal Buttons

We are using the Prime NG button classes for button styling

> We are not utilizing the Bootstrap button classes `.btn *`

Any button on page that is the main call to action should use the **Primary** class `.ui-button-primary` :

```
 <button pButton
        label="Save"
        class="ui-button-primary"
        ></button>
```

Any additional buttons that are not the main call to action should use the respective class:

- `.button-outline-secondary` - Cancel action or the alternative to the main call to action on a page
- `.ui-button-secondary` - Currently only used when attached to a textbox
- `.ui-button-danger` - Delete / remove actions
- `.ui-button-warning` - Remove actions
- `.ui-button-teal` - Primary call to action buttons that need to be colored in the Loadshop <span style="color:#31c2ae"> teal</span>
  > Note we also use this class for any button that has an **Add** action and use the following markup to add a font-awesome + icon to the button:

```
<button class="ui-button-primary ui-button-teal "
        pButton
        icon="pi pi-plus"
        (click)="addAddressLine()"
        label="Add Address Line"></button>
```

### Labels / Input Fields

All input fields must have a label, use the `.kbxl-inputgroup-container` class on the encompassing `<div>`:

```
<div class="kbxl-inputgroup-container">
  <div class="kbxl-field-label">
    <label class="control-label">Commodity</label>
  </div>
  <div>
    <p-dropdown ...attributes and properties>
    </p-dropdown>
  </div>
</div>

```

### Color Bars

Add a Loadshop color bar to the top of every page to start your specific content. They can also be placed to separate components:

```
<kbxl-color-bar></kbxl-color-bar>
```

### Progress Spinners

For any loading actions or delay in the UI, add a spinner to show waiting:

```
 <div class="progress-indicator" *ngIf="processing">
    <div class="progressspinner-container">
      <p-progressSpinner></p-progressSpinner>
    </div>
  </div>
```

### Dialogs / Toast Notifications

### Toast Notifications

Display Toast notifications after user actions are completed successfully or an error occurred. Use the Primg NG Message service `MessageService`:

```
import { MessageService } from 'primeng/components/common/messageservice';

constructor(private messageService: MessageService) { }

this.messageService.add({ id: 0, detail: 'Special Instruction Deleted', severity: 'success' });
```
