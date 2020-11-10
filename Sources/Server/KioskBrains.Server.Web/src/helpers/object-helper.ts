export class ObjectHelper {

  public static deepCopyViaJson(sourceInstance: any): any {
    return JSON.parse(JSON.stringify(sourceInstance));
  }

}
