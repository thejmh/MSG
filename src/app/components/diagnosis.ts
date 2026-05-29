import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataService, Question, DiagnosticResult, Acupoint, MeridianPoint } from '../services/data.service';
import { AnatomyMapComponent } from './anatomy-map';
import { AcupointDetailComponent } from './acupoint-detail';
import { animate } from 'motion';

interface PlottedPoint {
  acupoint: Acupoint;
  prescription: MeridianPoint;
}

@Component({
  selector: 'app-diagnosis',
  imports: [CommonModule, AnatomyMapComponent, AcupointDetailComponent],
  template: `
    <div class="min-h-screen bg-[#09090b] text-slate-100 flex flex-col items-center justify-start p-4 sm:p-8 font-sans selection:bg-emerald-500/30">
      <!-- Container -->
      <div class="max-w-xl w-full bg-[#121217]/85 backdrop-blur-xl rounded-[32px] border border-slate-800/80 shadow-2xl p-6 sm:p-8 mt-4 mb-8 relative overflow-hidden">
        
        <!-- Header -->
        <header class="mb-8 text-center relative z-10">
          <h1 class="text-2xl font-bold tracking-tight bg-gradient-to-r from-emerald-400 to-teal-500 bg-clip-text text-transparent">
            프로젝트 MSG
          </h1>
          <p class="text-slate-500 text-xs mt-1 font-medium tracking-wide uppercase">
            Meridian Symptom Guide
          </p>
          <div class="h-[2px] w-8 bg-gradient-to-r from-emerald-500 to-teal-500 mx-auto mt-3 rounded-full"></div>
        </header>

        <!-- Loading State -->
        @if (isLoading()) {
          <div class="flex flex-col items-center justify-center py-16">
            <div class="animate-spin rounded-full h-10 w-10 border-t-2 border-r-2 border-emerald-500"></div>
            <p class="mt-4 text-sm text-slate-400 font-medium">건강 지식 데이터베이스 로딩 중...</p>
          </div>
        }

        <!-- Questionnaire Screen -->
        @if (viewMode() === 'question') {
          @if (currentQuestion(); as q) {
            <div id="question-container" class="space-y-6 relative z-10">
              <!-- Back Button -->
              @if (historyStack().length > 0) {
                <button
                  (click)="handleBack()"
                  class="flex items-center gap-1 text-slate-400 hover:text-slate-200 text-xs font-semibold px-3 py-1.5 rounded-lg bg-slate-800/40 hover:bg-slate-800 border border-slate-700/50 transition-all duration-200"
                >
                  ← 이전 단계
                </button>
              }

              <h2 class="text-lg font-semibold text-slate-200 leading-snug text-center pt-2">
                {{ q.text }}
              </h2>

              <div class="grid gap-3 pt-4">
                @for (opt of q.options; track opt.text) {
                  <button 
                    (click)="handleAnswer(opt.nextId)"
                    class="w-full py-4 px-6 bg-slate-900/60 hover:bg-slate-800/80 text-slate-300 hover:text-white font-semibold rounded-2xl border border-slate-800 hover:border-emerald-500/40 transition-all duration-300 text-left flex justify-between items-center group shadow-md"
                  >
                    <span>{{ opt.text }}</span>
                    <span class="opacity-0 group-hover:opacity-100 group-hover:translate-x-1 transition-all duration-300 text-emerald-400">→</span>
                  </button>
                }
              </div>
            </div>
          }
        }

        <!-- Result Screen -->
        @else if (viewMode() === 'result') {
          @if (result(); as r) {
            <div id="result-container" class="text-center space-y-6 relative z-10">
              <div class="w-16 h-16 bg-emerald-500/10 text-emerald-400 rounded-2xl flex items-center justify-center mx-auto mb-4 border border-emerald-500/20">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>

              <h2 class="text-xl font-bold text-white tracking-tight">문진 진단 완료</h2>
              
              <div class="p-5 bg-slate-900/40 border border-slate-800/80 rounded-2xl">
                <span class="text-[10px] font-bold text-emerald-500 uppercase tracking-widest">추천 마사지 패키지</span>
                <p class="text-lg font-bold text-slate-100 mt-1">{{ r.title }}</p>
                <p class="text-xs text-slate-400 mt-2">
                  증상 완화에 도움이 되는 총 {{ r.pts.length }}개의 혈자리 처방이 매핑되었습니다.
                </p>
              </div>
              
              <div class="grid gap-3 pt-4">
                <button 
                  (click)="startGuide()"
                  class="w-full py-4 bg-emerald-600 hover:bg-emerald-500 text-white font-bold rounded-2xl shadow-lg shadow-emerald-950/20 transition-all duration-300 flex items-center justify-center gap-2 cursor-pointer border border-emerald-500/30"
                >
                  지압 가이드 시작하기
                </button>
                <button 
                  (click)="reset()"
                  class="text-slate-400 text-xs font-semibold hover:text-slate-200 transition-colors py-2"
                >
                  다시 진단하기
                </button>
              </div>
            </div>
          }
        }

        <!-- Interactive 2D Guide Screen -->
        @else if (viewMode() === 'guide') {
          <div id="guide-container" class="space-y-6 relative z-10">
            <!-- Header Bar -->
            <div class="flex justify-between items-center">
              <button
                (click)="viewMode.set('result')"
                class="flex items-center gap-1 text-slate-400 hover:text-slate-200 text-xs font-semibold px-3 py-1.5 rounded-lg bg-slate-800/40 hover:bg-slate-800 border border-slate-700/50 transition-all duration-200"
              >
                ← 진단 결과
              </button>
              <h2 class="text-sm font-bold text-slate-300">{{ result()?.title }}</h2>
            </div>

            <!-- Anatomy Map component -->
            <app-anatomy-map
              [points]="plottedPoints()"
              (pointSelected)="onPointSelected($event)"
            ></app-anatomy-map>

            <!-- Detailed Panel for Selected Acupoint -->
            @if (selectedPlottedPoint()) {
              <app-acupoint-detail
                [acupoint]="selectedPlottedPoint()!.acupoint"
                [prescription]="selectedPlottedPoint()!.prescription"
              ></app-acupoint-detail>
            }

            <div class="pt-2 text-center">
              <button 
                (click)="reset()"
                class="text-slate-500 text-xs font-semibold hover:text-slate-300 transition-colors py-2"
              >
                지압 종료하고 처음으로
              </button>
            </div>
          </div>
        }
      </div>
      
      <footer class="text-slate-600 text-[10px] tracking-widest uppercase text-center mt-2 font-mono">
        MSG Unified Angular v5.0
      </footer>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
  `]
})
export class DiagnosisComponent implements OnInit {
  private dataService = inject(DataService);

  isLoading = signal<boolean>(true);
  viewMode = signal<'question' | 'result' | 'guide'>('question');
  currentQuestion = signal<Question | null>(null);
  result = signal<DiagnosticResult | null>(null);
  historyStack = signal<string[]>([]);

  plottedPoints = signal<PlottedPoint[]>([]);
  selectedPlottedPoint = signal<PlottedPoint | null>(null);

  ngOnInit() {
    this.dataService.loadAllData().subscribe(() => {
      this.isLoading.set(false);
      const startNode = this.dataService.getQuestion('q_start');
      if (startNode) {
        this.currentQuestion.set(startNode);
      }
    });
  }

  handleAnswer(nextId: string) {
    const currentId = this.currentQuestion()?.id;
    if (currentId) {
      this.historyStack.update(stack => [...stack, currentId]);
    }

    if (nextId.startsWith('res_')) {
      const res = this.dataService.getDiagnosticResult(nextId);
      if (res) {
        this.result.set(res);
        this.viewMode.set('result');
        this.currentQuestion.set(null);
        this.animateIn('#result-container');
      }
    } else {
      const nextQ = this.dataService.getQuestion(nextId);
      if (nextQ) {
        this.currentQuestion.set(nextQ);
        this.animateIn('#question-container');
      }
    }
  }

  handleBack() {
    const stack = this.historyStack();
    if (stack.length === 0) return;

    const previousId = stack[stack.length - 1];
    const prevQ = this.dataService.getQuestion(previousId);
    
    if (prevQ) {
      this.historyStack.set(stack.slice(0, -1));
      this.currentQuestion.set(prevQ);
      this.animateIn('#question-container');
    }
  }

  startGuide() {
    const res = this.result();
    if (!res) return;

    const mapped: PlottedPoint[] = [];
    res.pts.forEach(pt => {
      const acupoint = this.dataService.getAcupoint(pt.id);
      if (acupoint) {
        mapped.push({
          acupoint,
          prescription: pt
        });
      }
    });

    this.plottedPoints.set(mapped);
    if (mapped.length > 0) {
      this.selectedPlottedPoint.set(mapped[0]);
    }
    
    this.viewMode.set('guide');
    this.animateIn('#guide-container');
  }

  onPointSelected(pt: PlottedPoint) {
    this.selectedPlottedPoint.set(pt);
  }

  reset() {
    this.result.set(null);
    this.historyStack.set([]);
    this.plottedPoints.set([]);
    this.selectedPlottedPoint.set(null);
    this.viewMode.set('question');
    
    const startNode = this.dataService.getQuestion('q_start');
    if (startNode) {
      this.currentQuestion.set(startNode);
      this.animateIn('#question-container');
    }
  }

  private animateIn(selector: string) {
    setTimeout(() => {
      const el = document.querySelector(selector);
      if (el) {
        animate(el, { opacity: [0, 1], y: [12, 0] }, { duration: 0.35, ease: 'easeOut' });
      }
    }, 0);
  }
}
