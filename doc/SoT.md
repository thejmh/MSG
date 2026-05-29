# 🧠 [SoT 마스터 문서] 프로젝트 MSG (Meridian Symptom Guide) v5.0 - Unified Angular Spec

## 1. Core Specification (핵심 명세)
* **시스템 아이덴티티:** 사용자의 모호한 신체적 고통을 결정론적 논리망(DAG)으로 진단하고, 도출된 경락/혈자리 솔루션을 고품질 인터랙티브 2D 해부학적 지도(Anatomical Map) 위에 시각화하여 개인화 지압 가이드를 제공하는 초개인화 무과금 건강 보조 웹 애플리케이션.
* **플랫폼 아키텍처 (Unified Single-Phase):**
  * **프레임워크:** Angular 21+, TypeScript 5+, RxJS 7+.
  * **스타일링:** TailwindCSS v4 (Modern & Premium Aesthetics).
  * **배포 규격:** PWA (Progressive Web App)를 기본 탑재하여 모바일 기기 홈 화면 추가 및 100% 오프라인 작동 지원.
  * **하이브리드 네이티브 폴백:** Capacitor 래핑을 지원하여 단일 코드베이스로 Google Play Store 및 Apple App Store 배포 가능.
* **데이터 백본 (Ground Truth):**
  * `결정 트리 (decision-tree.json)`: 증상 분류 및 진단 논리망 구조.
  * `진단 결과 (diagnostics.json)`: 진단 결과 메타 및 맞춤 처방(혈자리 조합, 세기, 방법).
  * `혈자리 DB (Acupoint DB)`: 361개 표준 혈자리의 한글명, 한자명, 속한 경락, 해부학적 위치 설명 및 부위별 2D 좌표.

---

## 2. Project Concept & Philosophy (프로젝트 철학)
* **단일 플랫폼 통합 (Single-Phase Integration):** 이전의 `Web + Unity AR` 이원화 구조에서 발생하던 딥링크 연동 지연, 디바이스 호환성 문제, 비정상 종료(Theme/Camera API 크래시) 위험을 제거하고 **Angular 단일 코드베이스**로 성능과 사용자 경험을 극대화한다.
* **결정론적 신뢰 (Absolute Determinism):** 생성형 AI의 환각(Hallucination) 개입을 배제하고 오직 정적 결정론적 논리망(DAG)을 통해 검증된 솔루션만 도출한다.
* **Zero-Cost 인프라:** 클라우드 서버나 유료 데이터베이스를 전혀 배제하고 정적 호스팅(GitHub Pages 등) 및 클라이언트 메모리 내 연산만으로 작동한다.
* **모바일 퍼스트 & 프리미엄 UI:** 폰에서 네이티브 앱처럼 동작하는 HSL 테마 기반 Sleek Dark Mode 및 Motion(Framer Motion 계열) 라이브러리를 활용한 마이크로 인터랙션을 제공한다.

---

## 3. Detailed Architecture (상세 아키텍처)

### A. Component Hierarchy (컴포넌트 구조)
* **`AppComponent`**: 전역 네비게이션 및 PWA 서비스 워커 관리.
* **`DiagnosisStateService`**: RxJS `BehaviorSubject`를 사용해 현재 문진 상태(뒤로가기 스택, 선택지 기록)를 유지하는 싱글톤 서비스.
* **`DataFetchService`**: `decision-tree.json`, `diagnostics.json` 등의 로컬 리소스를 비동기 로딩하고 인메모리 캐싱하는 공급자.
* **`QuestionnaireComponent (Smart)`**: 문진 진행 화면. 데이터 변경을 감지하여 UI를 제어함.
  * **`QuestionCardComponent (Dumb)`**: 질문 텍스트 출력 컴포넌트.
  * **`OptionGroupComponent (Dumb)`**: 선택지 출력 및 클릭 이벤트 방출 컴포넌트.
* **`AnatomyMapComponent (Interactive Guide)`**: 2D 인터랙티브 해부도 코어 컴포넌트.
  * 머리/얼굴, 상반신(앞/뒤), 팔/손, 하반신/발 등 처방에 맞는 부위의 SVG 일러스트 혹은 고해상도 해부학적 지도를 동적으로 렌더링.
  * 진단된 혈자리의 위치를 해부도 좌표(X, Y %)에 정밀 마킹하고 맥동(Pulsing) 애니메이션 제공.
* **`AcupointGuideComponent (Detail Panel)`**: 선택된 혈자리에 대한 마사지 상세 안내 가이드.
  * **세기(i) 시각화:** 강하게(Green, Deep Press) / 약하게(Red, Gentle Rub).
  * **방법(m) 시각화:** 누르기(Press), 문지르기(Rub), 두드리기(Tap)를 CSS 키프레임 애니메이션으로 제공하여 사용자가 쉽게 따라 할 수 있도록 설계.

---

## 4. UI/UX Design System (디자인 가이드라인)
* **Theme Palette (sleek Dark Mode):**
  * Background: `#0f0f11` (Deep Charcoal Grey)
  * Card Background: `#18181c` (Glassmorphism & Border line `#2a2a32` 적용)
  * Accent Primary: `#10b981` (Emerald Green - Deep Press 신호용)
  * Accent Secondary: `#ef4444` (Vibrant Red - Gentle Rub 신호용)
* **Typography:** `Inter` 또는 `Outfit` 구글 폰트를 활용한 미니멀하고 가독성 높은 레이아웃.
* **Transitions:** 라우팅 또는 문진 카드 전환 시 `TranslateX` 슬라이드 및 `Fade-in` 애니메이션 적용으로 네이티브 앱 느낌 구현.

---

## 5. Build & Deployment Plan (빌드 및 배포 계획)

### PWA 배포 파이프라인
1. `@angular/pwa` 패키지를 추가하여 `ngsw-config.json` 정의.
2. `decision-tree.json`, `diagnostics.json` 및 해부도 이미지 자산을 캐싱하여 완전 오프라인 구동 실현.
3. GitHub Pages 또는 Netlify 등을 활용해 정적 사이트로 배포.

### Capacitor 모바일 패키징 (선택 사항)
1. `@capacitor/core`, `@capacitor/cli` 및 안드로이드/iOS 플랫폼 패키지 추가.
2. `angular.json`의 빌드 출력 경로(`dist/app/browser`)를 Capacitor 설정(`capacitor.config.json`의 `webDir`)에 바인딩.
3. `npx cap sync` 명령어를 통해 네이티브 코드 동기화 후 Android Studio 또는 Xcode를 통해 네이티브 앱 빌드 수행.

---

## 6. 유지보수 AI 행동규칙
* Angular 21의 표준 문법과 RxJS 7+ 선언형 프로그래밍 방식을 엄격하게 적용할 것.
* 컴포넌트는 비즈니스 로직(Smart)과 순수 UI(Dumb) 역할을 명확히 구분할 것.
* CSS는 inline style을 지양하고, `TailwindCSS v4`의 유틸리티 클래스 및 CSS 변수 설계를 지향할 것.