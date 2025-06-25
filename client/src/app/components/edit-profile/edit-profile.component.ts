import { Component, inject } from '@angular/core';
import { LoggedInUser } from '../../models/account.model';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { MatTabChangeEvent, MatTabsModule } from '@angular/material/tabs';
import { AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { UserUpdate } from '../../models/user-update.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiResponse } from '../../models/helpers/apiResponse.model';

@Component({
  selector: 'app-edit-profile',
  imports: [
    SidebarComponent, MatTabsModule, FormsModule, ReactiveFormsModule
  ],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.scss'
})
export class EditProfileComponent {
  private _userService = inject(UserService);
  private _fB = inject(FormBuilder);
  private _snack = inject(MatSnackBar);
  isSidebarOpen = false;

  userFg = this._fB.group({
    bioCtrl: ''
  })

  get BioCtrl(): FormControl {
    return this.userFg.get('bioCtrl') as FormControl;
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
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
