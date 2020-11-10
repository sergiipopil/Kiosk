import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class BroadcastService {
    private _event: Subject<BroadcastEvent> = new Subject<BroadcastEvent>();

    public next(event: BroadcastEvent): void {
        return this._event.next(event);
    }

    public getEvents(eventNames: string[]): Observable<BroadcastEvent> {
        return this._event.asObservable()
            .filter(broadcastEvent => eventNames.indexOf(broadcastEvent.name) !== -1);
    }
}

export class BroadcastEvent {
    name: string;
    args?: any;
}
