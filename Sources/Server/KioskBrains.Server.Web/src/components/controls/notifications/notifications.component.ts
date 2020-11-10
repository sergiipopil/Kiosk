import { Component, Input } from '@angular/core';
import { NotificationRecord, NotificationTypeEnum } from 'models/base/notification-record';

@Component({
    selector: 'notifications',
    templateUrl: './notifications.component.html',
    styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent {

    @Input()
    notifications: NotificationRecord[];
    
    public removeNotification(index: number) {
        this.notifications.splice(index, 1);
    }

    // for template support
    NotificationTypeEnum = NotificationTypeEnum;
}
