import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';

export interface Option {
  text: string;
  nextId: string;
}

export interface Question {
  id: string;
  text: string;
  options: Option[];
  isResult?: boolean;
  resultId?: string;
}

export interface MeridianPoint {
  id: number;
  i: 0 | 1; // Intensity (1: Strong/Green, 0: Soft/Red)
  m: 1 | 2 | 3; // Method (1: Press, 2: Rub, 3: Tap)
}

export interface DiagnosticResult {
  id: string;
  title: string;
  pts: MeridianPoint[];
}

export interface Acupoint {
  id: number;
  meridian: string;
  name: string;
  hanja: string;
  page: number | null;
  symptoms: string;
  priority: string;
  location: string;
  region: 'head' | 'front_body' | 'back_body' | 'arm' | 'leg';
  x: number;
  y: number;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private http = inject(HttpClient);

  private questions: Question[] = [];
  private diagnostics: Record<string, DiagnosticResult> = {};
  private acupoints: Acupoint[] = [];
  private isLoaded = false;

  loadAllData(): Observable<boolean> {
    if (this.isLoaded) {
      return of(true);
    }

    return forkJoin({
      questions: this.http.get<Question[]>('logic-tree.json'),
      diagnosticsList: this.http.get<DiagnosticResult[]>('diagnostics.json'),
      acupoints: this.http.get<Acupoint[]>('acupoints.json')
    }).pipe(
      tap(({ questions, diagnosticsList, acupoints }) => {
        this.questions = questions;
        this.acupoints = acupoints;
        
        // Map diagnostics list to record dictionary
        this.diagnostics = {};
        diagnosticsList.forEach(item => {
          this.diagnostics[item.id] = item;
        });

        this.isLoaded = true;
      }),
      map(() => true)
    );
  }

  getQuestions(): Question[] {
    return this.questions;
  }

  getQuestion(id: string): Question | undefined {
    return this.questions.find(q => q.id === id);
  }

  getDiagnosticResult(id: string): DiagnosticResult | undefined {
    return this.diagnostics[id];
  }

  getAcupoint(id: number): Acupoint | undefined {
    return this.acupoints.find(a => a.id === id);
  }

  getAcupointsForRegion(region: string): Acupoint[] {
    return this.acupoints.filter(a => a.region === region);
  }
}
