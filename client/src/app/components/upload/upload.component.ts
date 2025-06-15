import { Component, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { LoggedInUser } from '../../models/account.model';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment.development';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { SidebarComponent } from "../sidebar/sidebar.component";

@Component({
  selector: 'app-upload',
  imports: [
    MatCardModule, FormsModule, MatIconModule, MatFormFieldModule,
    FileUploadModule, MatButtonModule, CommonModule,
    SidebarComponent
  ],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.scss'
})
export class UploadComponent implements OnInit {
  [x: string]: any;
  private _accountService = inject(AccountService);
  loggedInUser: LoggedInUser | null | undefined;
  uploader: FileUploader | undefined;
  apiUrl: string = environment.apiUrl;
  private _snack = inject(MatSnackBar);
  hasBaseDropZoneOver = false;
  isSidebarOpen = false;
  platformId = inject(PLATFORM_ID);

  constructor() {
    this.loggedInUser = this._accountService.loggedInUserSig();
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  fileOverBase(event: boolean): void {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(): void {
    if (this.loggedInUser) {
      this.uploader = new FileUploader({
        url: this.apiUrl + 'api/audiofile/upload',
        authToken: 'Bearer ' + this.loggedInUser.token,
        isHTML5: true,
        allowedFileType: ['audio'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 40 * 1024 * 1024
      });

      this.uploader.onAfterAddingFile = (file) => {
        file.withCredentials = false;
      };

      this.uploader.onSuccessItem = (item, response, status, headers) => {
        if (response) {
          console.log('Upload successful:', response);
          this._snack.open(response, "Close", {
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            duration: 10000
          });
        }
      };

      this.uploader.onErrorItem = (item, response, status, headers) => {
        this._snack.open(response, "Close", {
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          duration: 10000
        })
      };
    }
  }
}
