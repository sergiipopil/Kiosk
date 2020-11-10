import { Component, Input } from '@angular/core';
import { TabsComponent } from './tabs.component';

@Component({
  selector: 'tab',
  templateUrl: './tab.component.html'
})
export class TabComponent {

  constructor(tabs: TabsComponent) {
    tabs.addTab(this);
  }

  @Input('tab-title') title: string;

  active: boolean;
}
