import { Component, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./components/navbar/navbar.component";
import { FooterComponent } from "./components/footer/footer.component";
import { MatDividerModule } from '@angular/material/divider';
import { AnimatedWaveBackgroundComponent } from "./components/animated-wave-background/animated-wave-background.component";
import { RouterModule } from '@angular/router';
import { AccountService } from './services/account.service';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, NavbarComponent, MatDividerModule, AnimatedWaveBackgroundComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent  {
  private _accountService = inject(AccountService);
  private _platformId = inject(PLATFORM_ID);

  constructor() {
    if (isPlatformBrowser(this._platformId)) {
      const loggedInUserStr = localStorage.getItem('loggedInUser');
      
      if (loggedInUserStr) {        
        this._accountService.authorizeLoggedInUser();

        this._accountService.setCurrentUser(JSON.parse(loggedInUserStr))        
      }
    }
  }
}
