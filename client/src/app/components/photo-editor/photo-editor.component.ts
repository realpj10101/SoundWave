import { Component, inject, Input, OnInit } from '@angular/core';
import { LoggedInUser } from '../../models/account.model';
import { environment } from '../../../environments/environment.development';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../services/account.service';
import { UserService } from '../../services/user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Member } from '../../models/member.model';

@Component({
  selector: 'app-photo-editor',
  imports: [
    CommonModule,
    MatIconModule, MatFormFieldModule, MatCardModule, MatButtonModule,
    FileUploadModule, NgOptimizedImage
  ],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.scss'
})
export class PhotoEditorComponent implements OnInit {
  // @Input('memberInput') member: LoggedInUser | undefined; // from user-edit
  @Input('memberInput') member: Member | undefined;
  loggedInUser: LoggedInUser | null | undefined;
  errorGlob: string | undefined;
  apiUrl: string = environment.apiUrl;
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  private accountService = inject(AccountService);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);

  constructor() {
    this.loggedInUser = this.accountService.loggedInUserSig();
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(): void {
    if (this.loggedInUser) {
      this.uploader = new FileUploader({
        url: this.apiUrl + 'api/user/add-photo',
        authToken: 'Bearer ' + this.loggedInUser.token,
        isHTML5: true,
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 4_000_000, // bytes / 4MB
        // itemAlias: 'file'
      });

      this.uploader.onAfterAddingFile = (file) => {
        file.withCredentials = false;
      }

      this.uploader.onSuccessItem = (item, res) => {
        if (res) {
          const photo = JSON.parse(res);
          console.log(res);
          
          this.setNavbarProfilePhoto(photo)
        }
      }
    }
  }

  setNavbarProfilePhoto(url_165: string): void {
    if (this.loggedInUser) {

      this.loggedInUser.profilePhotoUrl = url_165;

      this.accountService.loggedInUserSig.set(this.loggedInUser);
    }
  }
}
