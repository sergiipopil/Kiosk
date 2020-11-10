import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'open-sidebar-button',
  templateUrl: './open-sidebar-button.component.html',
  styleUrls: ['./open-sidebar-button.component.scss']
})
export class OpenSidebarButtonComponent {
  
  private _opened: boolean;

  @Input()
  get opened(): boolean {
    return this._opened;
  }

  set opened(value: boolean) {
    this._opened = value;
    this.openedChange.emit(value);
  }

  @Output()
  openedChange = new EventEmitter<boolean>();

  @Input()
  label: string;
}
