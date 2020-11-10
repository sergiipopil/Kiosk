import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { KeyValue } from './key-value';

@Component({
  selector: 'additional-data-common-formatter',
  templateUrl: './additional-data-common-formatter.component.html'
})
export class AdditionalDataCommonFormatterComponent implements OnChanges {

  @Input()
  public additionalDataJson: string;

  ngOnChanges(changes: SimpleChanges): void {

    if (changes.additionalDataJson) {
      this.keyValues = [];
      try {
        let additionalData: any = JSON.parse(changes.additionalDataJson.currentValue);
        for (let propertyName in additionalData) {
          if (additionalData.hasOwnProperty(propertyName)) {
            this.keyValues.push({
              key: propertyName,
              value: additionalData[propertyName]
            });
          }
        }
      } catch (e) {
        console.error(e);
      } 
    }
  }

  public keyValues: KeyValue[];
}
