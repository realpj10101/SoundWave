import { Component, HostListener, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { LoggedInUser } from '../../models/account.model';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment.development';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { FormArray, FormBuilder, FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { GENRES, MOODS } from '../../models/types';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { generate } from 'rxjs';

@Component({
  selector: 'app-upload',
  imports: [
    MatCardModule, FormsModule, MatIconModule, MatFormFieldModule,
    FileUploadModule, MatButtonModule, CommonModule,
    SidebarComponent, ReactiveFormsModule, FormsModule
  ],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.scss'
})
export class UploadComponent {
  private _http = inject(HttpClient);
  private _snackbar = inject(MatSnackBar);
  private _accountService = inject(AccountService);
  private _fB = inject(FormBuilder);

  uploadedFile: File | null = null;
  coverImage: File | null | undefined;
  coverPreview = '';
  dragActive = false;
  uploadingProgress = 0;

  GENRES = GENRES;
  MOODS = MOODS;

  tagInput = '';

  submitFg = this._fB.group({
    fileNameCtrl: [''],
    genresCtrl: this._fB.array([]),
    moodsCtrl: this._fB.array([]),
    tagsCtrl: this._fB.array([]),
    tagsInputCtrl: ['']
  })

  get FileNameCtrl(): FormControl {
    return this.submitFg.get('fileNameCtrl') as FormControl;
  }

  get GenresCtrl(): FormArray {
    return this.submitFg.get('genresCtrl') as FormArray;
  }

  get MoodsCtrl(): FormArray {
    return this.submitFg.get('moodsCtrl') as FormArray;
  }

  get TagsCtrl(): FormArray {
    return this.submitFg.get('tagsCtrl') as FormArray;
  }

  get TagsInputCtrl(): FormControl {
    return this.submitFg.get('tagsInputCtrl') as FormControl;
  }

  toggleGenre(g: string): void {
    const idx = this.GenresCtrl.value.indexOf(g);

    if (idx === -1) this.GenresCtrl.push(this._fB.control(g));
    else this.GenresCtrl.removeAt(idx);
  }

  toggleMood(m: string): void {
    const idx = this.MoodsCtrl.value.indexOf(m);

    if (idx === -1) this.MoodsCtrl.push(this._fB.control(m));
    else this.MoodsCtrl.removeAt(idx);
  }

  addTag(): void {
    const t = (this.TagsInputCtrl.value || '').trim();

    console.log(t);

    if (!t) return;
    if (!this.TagsCtrl.value.includes(t)) {
      this.TagsCtrl.push(this._fB.control(t));
    }
    this.submitFg.get('tagInputCtrl')?.reset();
  }

  removeTag(tag: string): void {
    const idx = this.TagsCtrl.value.indexOf(tag);

    if (idx !== -1) this.TagsCtrl.removeAt(idx);
  }

  handleFileInput(ev: Event): void {
    const f = (ev.target as HTMLInputElement).files;

    if (f && f.length) {
      this.uploadedFile = f[0];

      if (!this.submitFg.value.fileNameCtrl) {
        this.submitFg.patchValue({
          fileNameCtrl: this.uploadedFile.name
        })
      }
    }
  }

  handleCoverImage(ev: Event): void {
    const f = (ev.target as HTMLInputElement).files;

    if (f && f.length) {
      this.coverImage = f[0];

      const reader = new FileReader();
      reader.onload = (e) => this.coverPreview = (e.target?.result as string) || '';
      reader.readAsDataURL(this.coverImage);
    }
  }

  removeCover(): void {
    this.coverImage = null;
    this.coverPreview = '';
  }

  handleDrag(e: DragEvent | Event): void {
    e.preventDefault();
    e.stopPropagation();

    if ((e as DragEvent).type === 'dragenter' || (e as DragEvent).type === 'dragover')
      this.dragActive = true;
    else if ((e as DragEvent).type === 'drageleave')
      this.dragActive = false;
  }

  handleDrop(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();

    this.dragActive = false;
    const dt = e.dataTransfer;

    if (!dt) return;

    const f = dt.files;

    if (f && f.length) {
      const file = f[0];
      if (file.type.startsWith('audio/')) {
        this.uploadedFile = file;
        if (!this.submitFg.value.fileNameCtrl) this.submitFg.patchValue({ fileNameCtrl: file.name });
      }
      else {
        this._snackbar.open('Please choose at least one audio file', 'Close', {
          verticalPosition: 'top',
          horizontalPosition: 'center',
          duration: 7000
        })
      }
    }
  }

  @HostListener('window:dragover', ['$event'])
  @HostListener('window:drop', ['$event'])
  preventWinsowDrop(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
  }

  submit(): void {
    if (!this.uploadedFile) {
      this._snackbar.open('No file selected', 'Clsoe', {
        horizontalPosition: 'center',
        verticalPosition: 'top',
        duration: 7000
      })
    }

    const fd = new FormData();
    fd.append('fileName', this.FileNameCtrl.value.trim() || this.uploadedFile?.name);

    if (this.uploadedFile)
      fd.append('file', this.uploadedFile, this.uploadedFile?.name);

    if (this.coverImage)
      fd.append('coverFile', this.coverImage, this.coverImage.name);

    console.log(this.GenresCtrl.value);

    (this.GenresCtrl.value || []).forEach((g: string) => fd.append('genres', g));
    (this.MoodsCtrl.value || []).forEach((m: string) => fd.append('moods', m));
    (this.TagsCtrl.value || []).forEach((t: string) => fd.append('tags', t));

    this.uploadingProgress = 0;
    this._http.post(environment.apiUrl + 'api/audiofile/upload', fd).subscribe({
      next: (res) => {
        this._snackbar.open('Upload successfull', 'Close', {
          duration: 7000,
          verticalPosition: 'top',
          horizontalPosition: 'center'
        })
        this.resetAll();
      },
      error: (err) => {
        const msg = err?.error?.message || 'Fail to upload';

        this._snackbar.open(msg, 'Close', {
          horizontalPosition: 'center',
          verticalPosition: 'top',
          duration: 7000
        })
      }
    })
  }

  resetAll(): void {
    this.submitFg.reset();
    while (this.GenresCtrl.length) this.GenresCtrl.removeAt(0);
    while (this.MoodsCtrl.length) this.MoodsCtrl.removeAt(0);
    while (this.TagsCtrl.length) this.TagsCtrl.removeAt(0);
    this.uploadedFile = null;
    this.coverImage = null;
    this.coverPreview = '';
    this.tagInput = '';
  }
}
