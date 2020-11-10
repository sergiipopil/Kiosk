import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { ComponentHistoryEntry } from 'models/component-history-entry';

@Injectable()
export class ModalService {
    public IsCurrentWindowModal: boolean;
    private nextModalSubject = new Subject<ComponentHistoryEntry>();
    public nextModalState = this.nextModalSubject.asObservable();

    private popModalSubject = new Subject<boolean>();
    public popModalState = this.popModalSubject.asObservable();

    private replaceModalSubject = new Subject<ComponentHistoryEntry>();
    public replaceModalState = this.replaceModalSubject.asObservable();

    addModal(component: ComponentHistoryEntry)
    {
        this.IsCurrentWindowModal = true;
        this.nextModalSubject.next(component);
    }

    removeModal(all: boolean=false) {
        if (all) {
            this.IsCurrentWindowModal = false;
        }

        this.popModalSubject.next(all);
    }

    replaceModal(component: ComponentHistoryEntry) {
        this.IsCurrentWindowModal = true;
        this.replaceModalSubject.next(component);
    }
}