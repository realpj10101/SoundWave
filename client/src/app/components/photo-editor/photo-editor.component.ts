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
    FileUploadModule
  ],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.scss'
})
export class PhotoEditorComponent {
}
