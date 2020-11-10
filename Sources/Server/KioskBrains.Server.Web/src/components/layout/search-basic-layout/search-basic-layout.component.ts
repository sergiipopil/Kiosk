import { Component, Input } from '@angular/core';

@Component({
  selector: 'search-basic-layout',
  templateUrl: './search-basic-layout.component.html',
  styleUrls: ['./search-basic-layout.component.scss']
})
export class SearchBasicLayoutComponent {
  @Input() showRightSidebar: boolean;
}
