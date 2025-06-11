import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnimatedWaveBackgroundComponent } from './animated-wave-background.component';

describe('AnimatedWaveBackgroundComponent', () => {
  let component: AnimatedWaveBackgroundComponent;
  let fixture: ComponentFixture<AnimatedWaveBackgroundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnimatedWaveBackgroundComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AnimatedWaveBackgroundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
