import { Component } from '@angular/core';

@Component({
  selector: 'app-register',
  imports: [],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  password: string = '';
  hide: boolean = true;

  togglePassword(): void {
    this.hide = !this.hide;
  }
}
