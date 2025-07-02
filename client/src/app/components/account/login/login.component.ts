import { Component, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../../services/account.service';
import { LoggedInUser, Login } from '../../../models/account.model';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-login',
  imports: [
    FormsModule, ReactiveFormsModule, MatFormFieldModule, MatButtonModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private _accountService = inject(AccountService);
  private _fb = inject(FormBuilder);

  password: string = '';
  hide: boolean = true;
  wrongUserNameOrPassword: string | undefined;

  loginFg = this._fb.group({
    emailCtrl: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
  })

  togglePassword(): void {
    this.hide = !this.hide;
  }

  get EmailCtrl(): FormControl {
    return this.loginFg.get('emailCtrl') as FormControl;
  }

  get PasswordCtrl(): FormControl {
    return this.loginFg.get('passwordCtrl') as FormControl;
  }

  login(): void {
    console.log('ok');
    
    let loginPlayer: Login = {
      email: this.EmailCtrl.value,
      password: this.PasswordCtrl.value
    }

    this._accountService.loginPlayer(loginPlayer).subscribe({      
      next: (loggedInPlayer: LoggedInUser | null) => {
        console.log(loggedInPlayer);
      },
      // show wrong username or password error.
      error: err => {
        this.wrongUserNameOrPassword = err.error;
      }
    })
  }
}
