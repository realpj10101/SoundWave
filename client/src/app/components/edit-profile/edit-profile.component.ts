import { Component, inject, OnInit, PLATFORM_ID, Signal } from '@angular/core';
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
import { MemberService } from '../../services/member.service';
import { Member } from '../../models/member.model';
import { take } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { AccountService } from '../../services/account.service';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { Photo } from '../../models/photo.model';

@Component({
  selector: 'app-edit-profile',
  imports: [
    SidebarComponent, MatTabsModule, FormsModule, ReactiveFormsModule,
    MatSlideToggleModule, MatIconModule, FileUploadModule
  ],
  templateUrl: './edit-profile.component.html',
  styleUrl: './edit-profile.component.scss'
})
export class EditProfileComponent implements OnInit {
  private _userService = inject(UserService);
  private _memberService = inject(MemberService);
  accountService = inject(AccountService);
  private _fB = inject(FormBuilder);
  private _snack = inject(MatSnackBar);
  isSidebarOpen = false;
  private platFormId = inject(PLATFORM_ID);
  loggedInUser: LoggedInUser | null | undefined;
  member: Member | undefined;
  apiUrl: string = environment.apiUrl;
  loggedInUserSig: Signal<LoggedInUser | null> | undefined;
  checked = false;
  disabled = false;
  uploader: FileUploader | undefined;

  userFg = this._fB.group({
    bioCtrl: ''
  })

  get BioCtrl(): FormControl {
    return this.userFg.get('bioCtrl') as FormControl;
  }
  
  ngOnInit(): void {
    this.loggedInUser = this.accountService.loggedInUserSig();

    this.initializeUploader();

    this.getMember();
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  initializeUploader(): void {
    if (this.loggedInUser) {
      this.uploader = new FileUploader({
        url: this.apiUrl + 'api/user/add-photo',
        authToken: 'Bearer ' + this.loggedInUser.token,
        isHTML5: true,
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: true,
        maxFileSize: 4_000_000, // bytes / 4MB
        // itemAlias: 'file'
      });

      this.uploader.onAfterAddingFile = (file) => {
        file.withCredentials = false;
      }

      this.uploader.onSuccessItem = (item, response, status, header) => {
        if (response) {
          const photo: Photo = JSON.parse(response);

          // set navbar profile photo when first photo is uploaded
          if (this.member?.photo)
            console.log('ok');
            
            this.setNavbarProfilePhoto(photo.url_165);
        }
      }
    }
  }

  setNavbarProfilePhoto(url_165: string): void {
    if (this.loggedInUser) {

      this.loggedInUser.profilePhotoUrl = url_165;

      this.accountService.setCurrentUser(this.loggedInUser);
    }
  }

  getMember(): void {
    if (isPlatformBrowser(this.platFormId)) {
      const loggedInUserStr: string | null = localStorage.getItem('loggedInUser');

      if (loggedInUserStr) {
        const loggedInUser: LoggedInUser = JSON.parse(loggedInUserStr);

        this._memberService.getByUserName(loggedInUser.userName)?.pipe(take(1)).subscribe(member => {
          if (member) {
            this.member = member;

            // this.initControllersValues(member);
          }
        });
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
