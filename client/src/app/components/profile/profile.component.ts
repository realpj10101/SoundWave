import { Component, ViewEncapsulation } from '@angular/core';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';

@Component({
  selector: 'app-profile',
  imports: [
    SidebarComponent, MatButtonModule, MatTabsModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  isSidebarOpen = false;

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
}
