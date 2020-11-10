// 'Notification' identifier is occupied by core
export interface NotificationRecord {
    type?: NotificationTypeEnum;
    message: string;
}

export enum NotificationTypeEnum {
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
}
