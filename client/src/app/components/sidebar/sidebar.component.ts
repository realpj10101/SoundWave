import { HttpClient } from '@angular/common/http';
import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  imports: [
    RouterModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  http = inject(HttpClient);
  @Input() isOpen = true;
  @Output() toggle = new EventEmitter<void>();

  getAll(): void {
    this.http.get('http://localhost:5000/api/audiofile').subscribe({
      next: (res) => console.log(res)
    });
  }
}