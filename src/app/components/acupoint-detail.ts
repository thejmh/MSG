import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Acupoint, MeridianPoint } from '../services/data.service';

@Component({
  selector: 'app-acupoint-detail',
  imports: [CommonModule],
  template: `
    @if (acupoint && prescription) {
      <div class="w-full bg-slate-900/50 border border-slate-800/80 rounded-3xl p-6 space-y-6">
        <!-- Acupoint Header -->
        <header class="flex justify-between items-start border-b border-slate-800 pb-4">
          <div>
            <div class="flex items-center gap-2">
              <h3 class="text-xl font-bold text-white tracking-tight">{{ acupoint.name }}</h3>
              <span class="text-xs font-semibold px-2 py-0.5 rounded-full bg-slate-800 text-slate-400">{{ acupoint.hanja }}</span>
            </div>
            <p class="text-xs text-emerald-500 mt-1 font-medium">{{ acupoint.meridian }}</p>
          </div>
          @if (acupoint.page) {
            <span class="text-[10px] uppercase font-mono px-2 py-1 rounded bg-slate-800 text-slate-500 border border-slate-700/50">
              P.{{ acupoint.page }}
            </span>
          }
        </header>

        <!-- Location Description -->
        <div class="space-y-1.5">
          <h4 class="text-xs font-semibold text-slate-400 uppercase tracking-wider">위치 설명</h4>
          <p class="text-sm text-slate-300 leading-relaxed">{{ acupoint.location }}</p>
        </div>

        <!-- Major Symptoms -->
        <div class="space-y-1.5">
          <h4 class="text-xs font-semibold text-slate-400 uppercase tracking-wider">주요 치료 증상</h4>
          <p class="text-sm text-slate-300 leading-relaxed">{{ acupoint.symptoms }}</p>
        </div>

        <!-- Instructions Grid -->
        <div class="grid grid-cols-2 gap-4 pt-2">
          <!-- Intensity Instruction -->
          <div class="bg-slate-950/60 border border-slate-800/50 rounded-2xl p-4 flex flex-col items-center justify-center text-center space-y-2">
            <span class="text-[10px] font-semibold text-slate-500 uppercase">지압 세기</span>
            
            <div class="flex items-center gap-2">
              <span 
                [class.bg-emerald-500]="prescription.i === 1"
                [class.bg-rose-500]="prescription.i === 0"
                class="w-3 h-3 rounded-full relative flex"
              >
                <span 
                  [class.bg-emerald-400]="prescription.i === 1"
                  [class.bg-rose-400]="prescription.i === 0"
                  class="animate-ping absolute inline-flex h-full w-full rounded-full opacity-75"
                ></span>
              </span>
              <span class="text-sm font-bold text-white">
                {{ prescription.i === 1 ? '강하게 (Deep)' : '부드럽게 (Soft)' }}
              </span>
            </div>
            <p class="text-[11px] text-slate-400">
              {{ prescription.i === 1 ? '지그시 깊게 누릅니다.' : '살살 문지르며 마사지합니다.' }}
            </p>
          </div>

          <!-- Method Instruction -->
          <div class="bg-slate-950/60 border border-slate-800/50 rounded-2xl p-4 flex flex-col items-center justify-center text-center space-y-2">
            <span class="text-[10px] font-semibold text-slate-500 uppercase">지압 방법</span>
            <span class="text-sm font-bold text-white">{{ getMethodLabel(prescription.m) }}</span>
            
            <!-- Animated Guide Box -->
            <div class="w-12 h-12 rounded-full border border-slate-800 bg-slate-900/50 flex items-center justify-center overflow-hidden">
              @if (prescription.m === 1) {
                <!-- Pulse animation for Press -->
                <div class="w-5 h-5 rounded-full bg-emerald-500/70 animate-press-pulse"></div>
              } @else if (prescription.m === 2) {
                <!-- Circle rotate animation for Rub -->
                <div class="w-6 h-6 rounded-full border-2 border-dashed border-rose-500/80 animate-spin-slow"></div>
              } @else if (prescription.m === 3) {
                <!-- Tap tap animation -->
                <div class="flex gap-1 items-end h-5">
                  <div class="w-1.5 h-3 bg-amber-500/80 rounded animate-tap-up-down-1"></div>
                  <div class="w-1.5 h-4 bg-amber-500/80 rounded animate-tap-up-down-2"></div>
                  <div class="w-1.5 h-3 bg-amber-500/80 rounded animate-tap-up-down-3"></div>
                </div>
              }
            </div>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes press-pulse {
      0%, 100% { transform: scale(0.7); opacity: 0.4; }
      50% { transform: scale(1.1); opacity: 1; }
    }
    @keyframes tap-up-down {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-8px); }
    }
    .animate-press-pulse {
      animation: press-pulse 1.6s ease-in-out infinite;
    }
    .animate-spin-slow {
      animation: spin 3s linear infinite;
    }
    .animate-tap-up-down-1 {
      animation: tap-up-down 0.8s ease-in-out infinite;
      animation-delay: 0s;
    }
    .animate-tap-up-down-2 {
      animation: tap-up-down 0.8s ease-in-out infinite;
      animation-delay: 0.25s;
    }
    .animate-tap-up-down-3 {
      animation: tap-up-down 0.8s ease-in-out infinite;
      animation-delay: 0.5s;
    }
  `]
})
export class AcupointDetailComponent {
  @Input() acupoint: Acupoint | null = null;
  @Input() prescription: MeridianPoint | null = null;

  getMethodLabel(method: number): string {
    switch (method) {
      case 1: return '누르기 (Press)';
      case 2: return '문지르기 (Rub)';
      case 3: return '두드리기 (Tap)';
      default: return '지압';
    }
  }
}
