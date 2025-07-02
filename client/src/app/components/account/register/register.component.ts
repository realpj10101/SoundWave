import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { AccountService } from '../../../services/account.service';
import { FormBuilder, FormControl, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { Register } from '../../../models/account.model';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-register',
  imports: [
    FormsModule, ReactiveFormsModule, MatFormFieldModule, MatButtonModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnDestroy {
  private _accountService = inject(AccountService);
  private _fb = inject(FormBuilder);

  passwordsNotMatch: boolean | undefined;
  subscribedRegisterPlayer: Subscription | undefined;
  emailExistError: string | undefined;
  password: string = '';
  hide: boolean = true;

  togglePassword(): void {
    this.hide = !this.hide;
  }

  ngOnDestroy(): void {
    this.subscribedRegisterPlayer?.unsubscribe();
  }

  registerFg = this._fb.group({
    emailCtrl: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^([\w\.\-]+)@([\w\-]+)((\.(\w){2,5})+)$/)]],
    userNameCtrl: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(30)]],
    passwordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]],
    confirmPasswordCtrl: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(20)]]
  })

  get EmailCtrl(): FormControl {
    return this.registerFg.get('emailCtrl') as FormControl;
  }

  get UserNameCtrl(): FormControl {
    return this.registerFg.get('userNameCtrl') as FormControl;
  }

  get PasswordCtrl(): FormControl {
    return this.registerFg.get('passwordCtrl') as FormControl;
  }

  get ConfirmPasswordCtrl(): FormControl {
    return this.registerFg.get('confirmPasswordCtrl') as FormControl;
  }

  register(): void {
    if (this.PasswordCtrl.value === this.ConfirmPasswordCtrl.value) {
      this.passwordsNotMatch = false;

      let regiterPlayer: Register = {
        email: this.EmailCtrl.value,
        userName: this.UserNameCtrl.value,
        password: this.PasswordCtrl.value,
      }

      this.subscribedRegisterPlayer = this._accountService.registerPlayer(regiterPlayer).subscribe({
        next: player => console.log(player),
        error: err => this.emailExistError = err.error
      });

      console.log(this.subscribedRegisterPlayer);
    }
    else {
      this.passwordsNotMatch = true;
    }
  }
}
