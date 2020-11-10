import { Component, Input } from '@angular/core';

@Component({
  selector: 'command-panel',
  templateUrl: './command-panel.component.html',
  styleUrls: ['./command-panel.component.scss']
})
export class CommandPanelComponent {
    @Input()
    show: boolean = true;
}
