import { Component, inject, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';
import { AccountService } from '../../services/account.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  imports: [
    MatButtonModule, MatDividerModule, RouterModule,
    CommonModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  accountService = inject(AccountService);
}
