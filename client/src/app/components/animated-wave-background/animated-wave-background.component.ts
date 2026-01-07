import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  Inject,
  OnDestroy,
  PLATFORM_ID,
  ViewChild
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-animated-wave-background',
  templateUrl: './animated-wave-background.component.html',
  styleUrls: ['./animated-wave-background.component.scss']
})
export class AnimatedWaveBackgroundComponent implements AfterViewInit, OnDestroy {
  @ViewChild('waveCanvas', { static: false }) canvasRef!: ElementRef<HTMLCanvasElement>;

  private _animationId: number = 0;
  private _time: number = 0;
  private _ctx!: CanvasRenderingContext2D | null;
  private _isBrowser: boolean;
  private _resizeHandler!: () => void;
  private _platformId = inject(PLATFORM_ID);

  constructor() {
    this._isBrowser = isPlatformBrowser(this._platformId);
  }

  ngAfterViewInit(): void {
    if (!this._isBrowser) return;
    setTimeout(() => this.startCanvasAnimation(), 0);
  }

  private startCanvasAnimation() {
    const canvas = this.canvasRef.nativeElement;
    this._ctx = canvas.getContext('2d');
    if (!this._ctx) return;

    this._resizeHandler = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };

    const drawWave = (
      amplitude: number,
      frequency: number,
      phase: number,
      yOffset: number,
      color: string,
      opacity: number
    ) => {
      const ctx = this._ctx!;
      ctx.beginPath();
      ctx.moveTo(0, canvas.height);

      for (let x = 0; x <= canvas.width; x++) {
        const y = yOffset + amplitude * Math.sin((x * frequency) / 100 + phase);
        if (x === 0) {
          ctx.moveTo(x, y);
        } else {
          ctx.lineTo(x, y);
        }
      }

      ctx.lineTo(canvas.width, canvas.height);
      ctx.lineTo(0, canvas.height);
      ctx.closePath();

      const gradient = ctx.createLinearGradient(0, yOffset - amplitude, 0, canvas.height);
      gradient.addColorStop(0, `${color}${Math.floor(opacity * 255).toString(16).padStart(2, '0')}`);
      gradient.addColorStop(0.3, `${color}${Math.floor(opacity * 0.8 * 255).toString(16).padStart(2, '0')}`);
      gradient.addColorStop(0.7, `${color}${Math.floor(opacity * 0.4 * 255).toString(16).padStart(2, '0')}`);
      gradient.addColorStop(1, `${color}00`);

      ctx.fillStyle = gradient;
      ctx.fill();
    };

    const animate = () => {
      if (!this._ctx) return;
      this._ctx.clearRect(0, 0, canvas.width, canvas.height);

      drawWave(120, 0.8, this._time * 0.007, canvas.height * 0.6, '#ea580c', 0.4);
      drawWave(150, 1.2, this._time * 0.005, canvas.height * 0.65, '#dc2626', 0.35);
      drawWave(180, 1.0, this._time * 0.008, canvas.height * 0.7, '#f97316', 0.3);
      drawWave(100, 1.5, this._time * 0.01, canvas.height * 0.75, '#b91c1c', 0.25);
      drawWave(200, 0.6, this._time * 0.006, canvas.height * 0.8, '#fb923c', 0.2);
      drawWave(80, 2.0, this._time * 0.01, canvas.height * 0.85, '#991b1b', 0.15);

      this._time += 1;
      this._animationId = requestAnimationFrame(animate);
    };

    this._resizeHandler();
    animate();
    window.addEventListener('resize', this._resizeHandler);
  }

  ngOnDestroy(): void {
    if (this._isBrowser) {
      cancelAnimationFrame(this._animationId);
      if (this._resizeHandler) {
        window.removeEventListener('resize', this._resizeHandler);
      }
    }
  }
}
