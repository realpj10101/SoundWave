import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./components/navbar/navbar.component";
import { FooterComponent } from "./components/footer/footer.component";
import { MatDividerModule } from '@angular/material/divider';
import { AnimatedWaveBackgroundComponent } from "./components/animated-wave-background/animated-wave-background.component";
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, NavbarComponent, FooterComponent, MatDividerModule, AnimatedWaveBackgroundComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'client';
}
