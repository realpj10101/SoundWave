import { Component, inject, PLATFORM_ID } from '@angular/core';
import { LoggedInUser } from '../../models/account.model';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { UserUpdate } from '../../models/user-update.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiResponse } from '../../models/helpers/apiResponse.model';
import { isPlatformBrowser } from '@angular/common';
import { PhotoEditorComponent } from "../photo-editor/photo-editor.component";

@Component({
  selector: 'app-edit-profile',
  imports: [
    SidebarComponent, MatTabsModule, FormsModule, ReactiveFormsModule,
    PhotoEditorComponent
],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.scss'
})
export class EditProfileComponent {
  private _userService = inject(UserService);
  private _fB = inject(FormBuilder);
  private _snack = inject(MatSnackBar);
  isSidebarOpen = false;
  private platformId = inject(PLATFORM_ID);
  loggedInUser: LoggedInUser | undefined;

  userFg = this._fB.group({
    bioCtrl: ''
  })

  get BioCtrl(): FormControl {
    return this.userFg.get('bioCtrl') as FormControl;
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  getUser(): void {
    if (isPlatformBrowser(this.platformId)) {
      const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

      if (loggedInUserStr) {
        this.loggedInUser = JSON.parse(loggedInUserStr);
      }
    }
  }

  updateUser(): void {
    let userUpdate: UserUpdate = {
      bio: this.BioCtrl.value
    }

    this._userService.updateUser(userUpdate).subscribe({
      next: (res: ApiResponse) => {
        if (res) {
          this._snack.open(res.message, "Close", {
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            duration: 10000
          });
        }
      }
    })
  }
}
