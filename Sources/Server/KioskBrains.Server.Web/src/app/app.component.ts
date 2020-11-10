import { Component } from '@angular/core';
import { ErrorService } from 'services/error.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'app';

  constructor(errorService: ErrorService) {
  }
}
