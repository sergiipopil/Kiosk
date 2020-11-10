import { Component, Input, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { ModalService } from 'services/modal.service';
import { ComponentHistoryEntry } from 'models/component-history-entry';

@Component({
  selector: 'modal-outlet',
  templateUrl: 'modal-outlet.component.html',
})
export class ModalOutletComponent implements AfterViewInit {
  modals: ComponentHistoryEntry[];

  constructor(private modalService: ModalService, private changeDetector: ChangeDetectorRef) {
      this.modals = [];
  }

  addModal(modal: ComponentHistoryEntry) {
      this.modals.push(modal);
      this.changeDetector.detectChanges();
  }

  popModal(all: boolean) {
      if (all) {
          this.modals = [];
          this.modalService.IsCurrentWindowModal = false;
      }
      else {
          this.modalService.IsCurrentWindowModal = this.modals.length > 1;
          this.modals.pop();
      }
      this.changeDetector.detectChanges();
  }

  replaceModal(modal: ComponentHistoryEntry) {
      this.modals[this.modals.length - 1] = modal;
      this.changeDetector.detectChanges();
  }

  ngAfterViewInit() {
      this.modalService.nextModalState.subscribe(val => this.addModal(val));
      this.modalService.replaceModalState.subscribe(val => this.replaceModal(val));
      this.modalService.popModalState.subscribe(val => this.popModal(val));
  }
}
