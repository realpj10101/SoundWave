import { Component, inject, PLATFORM_ID } from '@angular/core';
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
  imports: [RouterOutlet, RouterModule, NavbarComponent, FooterComponent, MatDividerModule, AnimatedWaveBackgroundComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private _accountService = inject(AccountService);
  private _platformId = inject(PLATFORM_ID);

  ngOnInit(): void {
    this.initUserOnPageRefresh();
  }

  initUserOnPageRefresh(): void {
    if (isPlatformBrowser(this._platformId)) {
      const loggedInPlayerStr = localStorage.getItem('loggedInUser');
      // const currentTeam = localStorage.getItem('currentTeam');

      if (loggedInPlayerStr) {
        // First, check if user's token is not expired.
        this._accountService.authorizeLoggedInPlayer();

        // Then, set the authorized logged-in user
        this._accountService.setCurrentPlayer(JSON.parse(loggedInPlayerStr))
      }
    }
  }
}
