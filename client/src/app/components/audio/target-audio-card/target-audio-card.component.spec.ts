import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TargetAudioCardComponent } from './target-audio-card.component';

describe('TargetAudioCardComponent', () => {
  let component: TargetAudioCardComponent;
  let fixture: ComponentFixture<TargetAudioCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TargetAudioCardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TargetAudioCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
