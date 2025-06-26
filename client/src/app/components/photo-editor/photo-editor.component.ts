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
import { ApiResponse } from '../../models/helpers/apiResponse.model';
import { take } from 'rxjs';
import { Photo } from '../../models/photo.model';

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
  @Input('memberInput') member: Member | undefined;
  loggedInUser: LoggedInUser | null | undefined;
  errorGlob: string | undefined;
  apiUrl: string = environment.apiUrl;
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  private _accountService = inject(AccountService);
  private _userService = inject(UserService);
  private _snackBar = inject(MatSnackBar);

  constructor() {
    this.loggedInUser = this._accountService.loggedInUserSig();
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

      this.uploader.onSuccessItem = (item, response, status, header) => {
        if (response) {
          const photo: Photo = JSON.parse(response);
          this.member?.photos.push(photo);

          console.log(photo);

          // set navbar profile photo when first photo is uploaded
          if (this.member?.photos.length === 1)
            this.setNavbarProfilePhoto(photo.url_165);
        }
      }
    }
  }

  setNavbarProfilePhoto(url_165: string): void {
    if (this.loggedInUser) {

      this.loggedInUser.profilePhotoUrl = url_165;

      this._accountService.loggedInUserSig.set(this.loggedInUser);
    }
  }

  setMainPhotoComp(url_165In: string): void {

    this._userService.setMainPhoto(url_165In)
      .pipe(take(1))
      .subscribe({
        next: (res: ApiResponse) => {
          if (res && this.member) {

            for (const photo of this.member.photos) {
              // unset user previous main photo
              if (photo.isMain === true)
                photo.isMain = false;

              if (photo.url_165 === url_165In) {
                photo.isMain = true;

                this.loggedInUser!.profilePhotoUrl = url_165In;
                this._accountService.setCurrentUser(this.loggedInUser!);
                console.log(this.loggedInUser);

              }
            }

            this._snackBar.open(res.message, 'close', {
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              duration: 7000
            });

            console.log(this.member.photos);
          }
        }
      });
  }

  deletePhotoComp(url_165In: string, index: number): void {
    this._userService.deletePhoto(url_165In)
      .pipe(take(1))
      .subscribe({
        next: (res: ApiResponse) => {
          if (res && this.member) {
            this.member.photos.splice(index, 1);

            this._snackBar.open(res.message, 'close', {
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              duration: 7000
            });
          }
        }
      })
  }
}
