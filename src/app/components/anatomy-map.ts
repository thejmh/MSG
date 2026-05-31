import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Acupoint, MeridianPoint } from '../services/data.service';

interface PlottedPoint {
  acupoint: Acupoint;
  prescription: MeridianPoint;
}

@Component({
  selector: 'app-anatomy-map',
  imports: [CommonModule],
  template: `
    <div class="flex flex-col items-center w-full space-y-6">
      <!-- Region Selector Tabs -->
      <div class="flex flex-wrap gap-2 justify-center w-full">
        @for (region of availableRegions(); track region) {
          <button
            (click)="selectRegion(region)"
            [class.bg-emerald-600]="activeRegion() === region"
            [class.text-white]="activeRegion() === region"
            [class.bg-slate-800]="activeRegion() !== region"
            [class.text-slate-300]="activeRegion() !== region"
            class="px-4 py-2 rounded-xl text-sm font-semibold transition-all duration-200 border border-slate-700 hover:border-slate-500"
          >
            {{ getRegionLabel(region) }}
            <span class="ml-1 text-xs px-1.5 py-0.5 rounded-full bg-slate-900/40 text-slate-400">
              {{ getPointsCountForRegion(region) }}
            </span>
          </button>
        }
      </div>

      <!-- Interactive Map Frame -->
      <div class="relative w-full aspect-square max-w-[360px] bg-slate-950/70 border border-slate-800/80 rounded-3xl overflow-hidden shadow-2xl flex items-center justify-center p-4">
        <!-- Background anatomy outline PNG based on active region -->
        <div class="absolute inset-0 flex items-center justify-center pointer-events-none p-2">
          <img 
            [src]="getRegionImage(activeRegion())" 
            class="w-full h-full object-contain opacity-45 select-none" 
            alt="Anatomy Guide Map"
          />
        </div>

        <!-- Plotted points -->
        @for (pt of currentPoints(); track pt.acupoint.id) {
          <button
            (click)="selectPoint(pt)"
            [style.top.%]="pt.acupoint.y"
            [style.left.%]="pt.acupoint.x"
            [class.ring-4]="selectedPoint()?.acupoint?.id === pt.acupoint.id"
            [class.ring-white]="selectedPoint()?.acupoint?.id === pt.acupoint.id"
            [class.bg-emerald-500]="pt.prescription.i === 1"
            [class.bg-rose-500]="pt.prescription.i === 0"
            class="absolute w-6 h-6 rounded-full flex items-center justify-center -translate-x-1/2 -translate-y-1/2 shadow-lg shadow-black/50 transition-all duration-300 transform hover:scale-125 z-20 cursor-pointer"
          >
            <!-- Pulse indicator -->
            <span 
              [class.bg-emerald-400]="pt.prescription.i === 1"
              [class.bg-rose-400]="pt.prescription.i === 0"
              class="animate-ping absolute inline-flex h-full w-full rounded-full opacity-75"
            ></span>
            <span class="text-[9px] font-extrabold text-slate-950 select-none">{{ pt.acupoint.name[0] }}</span>
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `]
})
export class AnatomyMapComponent implements OnInit, OnChanges {
  @Input() points: PlottedPoint[] = [];
  @Output() pointSelected = new EventEmitter<PlottedPoint>();

  availableRegions = signal<string[]>([]);
  activeRegion = signal<string>('head');
  currentPoints = signal<PlottedPoint[]>([]);
  selectedPoint = signal<PlottedPoint | null>(null);

  ngOnInit() {
    this.updateRegions();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['points']) {
      this.updateRegions();
    }
  }

  private updateRegions() {
    if (this.points && this.points.length > 0) {
      // Find all unique regions in the prescription acupoints
      const regions = Array.from(new Set(this.points.map(p => p.acupoint.region)));
      this.availableRegions.set(regions);
      
      // Auto select the first region that has points
      if (regions.length > 0) {
        this.selectRegion(regions[0]);
      }
    } else {
      this.availableRegions.set([]);
      this.currentPoints.set([]);
      this.selectedPoint.set(null);
    }
  }

  selectRegion(region: string) {
    this.activeRegion.set(region);
    const regionPoints = this.points.filter(p => p.acupoint.region === region);
    this.currentPoints.set(regionPoints);
    
    // Auto-select the first point in the new region
    if (regionPoints.length > 0) {
      this.selectPoint(regionPoints[0]);
    }
  }

  selectPoint(pt: PlottedPoint) {
    this.selectedPoint.set(pt);
    this.pointSelected.emit(pt);
  }

  getRegionLabel(region: string): string {
    switch (region) {
      case 'head': return '머리 / 얼굴';
      case 'front_body': return '가슴 / 배';
      case 'back_body': return '등 / 허리';
      case 'arm': return '팔 / 손';
      case 'leg': return '다리 / 발';
      default: return '기타';
    }
  }

  getPointsCountForRegion(region: string): number {
    return this.points.filter(p => p.acupoint.region === region).length;
  }

  private getBaseHref(): string {
    if (typeof document === 'undefined') return '';
    const baseEl = document.getElementsByTagName('base')[0];
    return baseEl ? baseEl.getAttribute('href') || '' : '';
  }

  getRegionImage(region: string): string {
    const base = this.getBaseHref();
    switch (region) {
      case 'head': return `${base}head_anatomy.png`;
      case 'front_body': return `${base}front_body_anatomy.png`;
      case 'back_body': return `${base}back_body_anatomy.png`;
      case 'arm': return `${base}arm_anatomy.png`;
      case 'leg': return `${base}leg_anatomy.png`;
      default: return `${base}head_anatomy.png`;
    }
  }
}
