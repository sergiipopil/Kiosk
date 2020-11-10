import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'confirm-message',
  templateUrl: 'confirm-message.component.html'
})
export class ConfirmMessageComponent {
  @Input() header: string;
  @Input() message: string;
  @Input() yes: Function;
  @Input() no: Function;
  @Input() close: Function;

  yesClick() {
    if (this.yes) {
      this.yes();
    }
  }

  noClick() {
    if (this.no) {
      this.no();
    }
  }

  closeClick() {
    if (this.close) {
      this.close();
    }
  }
}
