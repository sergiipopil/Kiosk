import { PortalKioskStateSearchRecord, ComponentStatusCodeEnum } from 'models/api/api';

export class KioskStatusDecorator {
  aliveComponentStatus: ComponentStatusCodeEnum;
  aliveComponentMessage: string;

  constructor(public kioskStateRecord: PortalKioskStateSearchRecord) {

    // last updated
    let lastPingedMinutesAgo = kioskStateRecord.lastPingedMinutesAgo;
    if (lastPingedMinutesAgo === null) {
      this.aliveComponentStatus = ComponentStatusCodeEnum.Disabled;
      this.aliveComponentMessage = 'Never Alive';
    } else {
      if (lastPingedMinutesAgo < 15) {
        this.aliveComponentStatus = ComponentStatusCodeEnum.Ok;
      } else {
        this.aliveComponentStatus = ComponentStatusCodeEnum.Error;
      }
      if (lastPingedMinutesAgo < 60) {
        // ReSharper disable QualifiedExpressionMaybeNull
        this.aliveComponentMessage = lastPingedMinutesAgo.toFixed(1) + ' minutes ago';
        // ReSharper restore QualifiedExpressionMaybeNull
      } else {
        let hoursAgo = lastPingedMinutesAgo / 60;
        if (hoursAgo < 24) {
          this.aliveComponentMessage = hoursAgo.toFixed(1) + ' hours ago';
        } else {
          let daysAgo = hoursAgo / 24;
          this.aliveComponentMessage = daysAgo.toFixed(1) + ' days ago';
        }
      }
    }
  }
}
