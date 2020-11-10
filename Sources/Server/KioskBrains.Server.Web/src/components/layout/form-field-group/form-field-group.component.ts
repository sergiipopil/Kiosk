import { Component, Input } from '@angular/core';

@Component({
  selector: 'form-field-group',
  templateUrl: './form-field-group.component.html',
  styleUrls: ['./form-field-group.component.scss']
})
export class FormFieldGroupComponent {
  @Input('group-title') title: string;
}
