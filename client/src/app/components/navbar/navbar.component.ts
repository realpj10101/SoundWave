import { Component, inject, OnInit, Signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';
import { AccountService } from '../../services/account.service';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { LoggedInUser } from '../../models/account.model';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-navbar',
  imports: [
    MatButtonModule, MatDividerModule, RouterModule,
    CommonModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent implements OnInit {
  accountService = inject(AccountService);
  loggedInUserSig: Signal<LoggedInUser | null> | undefined;
  apiUrl: string = environment.apiUrl;

  ngOnInit(): void {
      this.loggedInUserSig = this.accountService.loggedInUserSig;
  }
}
