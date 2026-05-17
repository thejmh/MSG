import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { HandoffService, HandoffPayload } from '../services/handoff';
import { animate } from 'motion';

interface Option {
  text: string;
  nextId: string;
}

interface Question {
  id: string;
  text: string;
  options: Option[];
}

@Component({
  selector: 'app-diagnosis',
  imports: [CommonModule, HttpClientModule],
  template: `
    <div class="min-h-screen bg-slate-50 flex flex-col items-center justify-center p-6 font-sans">
      <div class="max-w-md w-full bg-white rounded-3xl shadow-xl p-8 border border-slate-100">
        <header class="mb-8 text-center">
          <h1 class="text-2xl font-bold text-slate-900 tracking-tight">프로젝트 MSG</h1>
          <p class="text-slate-500 text-sm mt-1">개인화 AR 경락 마사지 가이드</p>
          <div class="h-1 w-12 bg-emerald-500 mx-auto mt-4 rounded-full"></div>
        </header>

        @if (currentQuestion(); as q) {
          <div id="question-container" class="space-y-6">
            <h2 class="text-lg font-medium text-slate-800 text-center mb-8">{{ q.text }}</h2>
            <div class="grid gap-3">
              @for (opt of q.options; track opt.text) {
                <button 
                  (click)="handleAnswer(opt.nextId)"
                  class="w-full py-4 px-6 bg-slate-50 hover:bg-emerald-50 text-slate-700 hover:text-emerald-700 font-medium rounded-2xl border border-slate-200 hover:border-emerald-200 transition-all duration-200 text-left flex justify-between items-center group"
                >
                  {{ opt.text }}
                  <span class="opacity-0 group-hover:opacity-100 transition-opacity">→</span>
                </button>
              }
            </div>
          </div>
        } @else if (result(); as r) {
          <div id="result-container" class="text-center space-y-6">
            <div class="w-16 h-16 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 class="text-xl font-bold text-slate-900">진단 완료</h2>
            <p class="text-slate-600">추천 패키지: <span class="font-bold text-slate-800">{{ r.title }}</span></p>
            
            <button 
              (click)="launchAR()"
              class="w-full py-4 bg-emerald-600 hover:bg-emerald-700 text-white font-bold rounded-2xl shadow-lg shadow-emerald-200 transition-all duration-200 flex items-center justify-center gap-2"
            >
              AR 가이드 시작하기
            </button>
            <button 
              (click)="reset()"
              class="text-slate-400 text-sm hover:text-slate-600 transition-colors"
            >
              다시 진단하기
            </button>
          </div>
        } @else {
          <div class="flex flex-col items-center justify-center py-12">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600"></div>
            <p class="mt-4 text-slate-500">데이터를 불러오는 중...</p>
          </div>
        }
      </div>
      
      <footer class="mt-8 text-slate-400 text-xs tracking-widest uppercase">
        Phase 1: Deterministic Logic Layer
      </footer>
    </div>
  `
})
export class DiagnosisComponent implements OnInit {
  private http = inject(HttpClient);
  private handoff = inject(HandoffService);

  questions = signal<Question[]>([]);
  currentQuestion = signal<Question | null>(null);
  result = signal<any | null>(null);

  ngOnInit() {
    this.http.get<Question[]>('/logic-tree.json').subscribe(data => {
      this.questions.set(data);
      this.currentQuestion.set(data[0]);
    });
  }

  handleAnswer(nextId: string) {
    if (nextId.startsWith('res_')) {
      // It's a result
      this.http.get<any>('/diagnostics.json').subscribe(allResults => {
        this.result.set(allResults[nextId]);
        this.currentQuestion.set(null);
        this.animateIn('#result-container');
      });
    } else {
      // It's another question
      const nextQ = this.questions().find(q => q.id === nextId);
      if (nextQ) {
        this.currentQuestion.set(nextQ);
        this.animateIn('#question-container');
      }
    }
  }

  launchAR() {
    const res = this.result();
    if (res) {
      const payload: HandoffPayload = {
        dId: res.id,
        pts: res.pts
      };
      this.handoff.launchARGuide(payload);
    }
  }

  reset() {
    this.result.set(null);
    this.currentQuestion.set(this.questions()[0]);
  }

  private animateIn(selector: string) {
    setTimeout(() => {
      const el = document.querySelector(selector);
      if (el) {
        animate(el, { opacity: [0, 1], y: [10, 0] }, { duration: 0.4 });
      }
    }, 0);
  }
}
