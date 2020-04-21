import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { SelectItem } from 'primeng/api';

@Component({
  selector: 'kbxl-dropdown',
  template: `
    <p-dropdown
      [(ngModel)]="selectedItem"
      [options]="selectItems"
      [placeholder]="placeholder"
      [styleClass]="styleClass"
      dropdownIcon="pi pi-caret-down"
      appendTo="body"
      kbxlCloseOnScroll
      hideTransitionOptions="0ms"
      [disabled]="disabled"
    >
      <ng-template let-item pTemplate="selectedItem">
        <span class="selected-item-label">{{ optionLabel(item?.value) }}</span>
      </ng-template>
    </p-dropdown>
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownComponent),
      multi: true,
    },
  ],
})
export class DropdownComponent implements ControlValueAccessor {
  @Input() placeholder: string;
  @Input() labelMember: string;
  @Input() styleClass: string;
  @Input() appendTo: string;

  public disabled = false;
  private _selectedId: any;
  private _selectItems: SelectItem[];
  private _selectedItem: any;

  private _idMember: string;
  get idMember() {
    return this._idMember;
  }
  @Input() set idMember(value: string) {
    this._idMember = value;
    this.updateSelectedItem();
  }

  public get selectedItem() {
    return this._selectedItem;
  }
  public set selectedItem(value: any) {
    this._selectedItem = value;

    this._selectedId = value ? value[this.idMember] : null;
    this._onChange(this._selectedId);
  }

  public optionLabel(option: any) {
    const label = option ? option[this.labelMember] : null;
    return label ? label.toString() : null;
  }

  @Input() set options(value: any) {
    this._selectItems = value ? value.map(_ => <SelectItem>{ label: this.optionLabel(_), value: _ }) : null;
    this.updateSelectedItem();
  }
  get selectItems() {
    return this._selectItems;
  }

  _onChange = (id: any) => {};
  _onTouched = () => {};

  constructor() {}

  writeValue(value: any): void {
    if (this._selectedId !== value) {
      this._selectedId = value;
      this.updateSelectedItem();
    }
  }

  private updateSelectedItem() {
    if (this.idMember) {
      const item =
        !this.selectItems || !this._selectedId
          ? null
          : this.selectItems.find(x => {
              return x.value[this.idMember] === this._selectedId;
            });
      this.selectedItem = item ? item.value : null;
    }
  }

  registerOnChange(fn: (id: string) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
