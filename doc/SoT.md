# 🧠 [SoT 마스터 문서] 프로젝트 MSG (Meridian Symptom Guide) v5.0 - Unified Angular Spec

## 1. Core Specification (핵심 명세)
* **시스템 아이덴티티:** 사용자의 모호한 신체적 고통을 결정론적 논리망(DAG)으로 진단하고, 도출된 경락/혈자리 솔루션을 고품질 인터랙티브 2D 해부학적 지도(Anatomy Map) 위에 시각화하여 개인화 지압 가이드를 제공하는 초개인화 무과금 건강 보조 웹 애플리케이션.
* **플랫폼 아키텍처 (Unified Single-Phase):**
  * **프레임워크:** Angular 21+, TypeScript 5+, RxJS 7+.
  * **스타일링:** TailwindCSS v4 (Modern & Premium Aesthetics).
  * **자산 레이아웃:** 고화질 네온 청록색(Neon Teal) 테마 해부도 PNG 5종 적용 (`/head_anatomy.png`, `/front_body_anatomy.png`, `/back_body_anatomy.png`, `/arm_anatomy.png`, `/leg_anatomy.png`).
  * **배포 규격:** PWA (Progressive Web App)를 기본 탑재하여 모바일 기기 홈 화면 추가 및 100% 오프라인 작동 지원.
  * **하이브리드 네이티브 폴백:** Capacitor 래핑을 지원하여 단일 코드베이스로 Google Play Store 및 Apple App Store 배포 가능.
* **데이터 백본 (Ground Truth):**
  * `결정 트리 (logic-tree.json)`: 증상 분류 및 진단 논리망 구조 (10개 질문 노드, 26개 결과 노드).
  * `진단 결과 (diagnostics.json)`: 진단 결과 메타 및 맞춤 처방(혈자리 조합, 세기 `i`, 방법 `m`).
  * `혈자리 DB (acupoints.json)`: 361개 표준 혈자리의 이름, 한자명, 속한 경락, 해부학적 위치 설명, 주요 치료 증상 및 각 인체 지도별 X/Y(%) 위치 좌표.

---

## 2. Project Concept & Philosophy (프로젝트 철학)
* **단일 플랫폼 통합 (Single-Phase Integration):** 이전의 `Web + Unity AR` 이원화 구조에서 발생하던 딥링크 연동 지연, 디바이스 호환성 문제, 비정상 종료(Theme/Camera API 크래시) 위험을 제거하고 **Angular 단일 코드베이스**로 성능과 사용자 경험을 극대화한다.
* **결정론적 신뢰 (Absolute Determinism):** 생성형 AI의 환각(Hallucination) 개입을 배제하고 오직 정적 결정론적 논리망(DAG)을 통해 검증된 솔루션만 도출한다.
* **Zero-Cost 인프라:** 클라우드 서버나 유료 데이터베이스를 전혀 배제하고 정적 호스팅(GitHub Pages 등) 및 클라이언트 메모리 내 연산 및 캐시만으로 작동한다.
* **모바일 퍼스트 & 프리미엄 UI:** 폰에서 네이티브 앱처럼 동작하는 HSL 테마 기반 Sleek Dark Mode 및 Motion(Framer Motion 계열) 라이브러리를 활용한 마이크로 인터랙션을 제공한다.

---

## 3. Detailed Architecture (상세 아키텍처)

### A. Component Hierarchy (컴포넌트 구조)
* **`AppComponent`**: 전역 네비게이션 및 PWA 서비스 워커 관리.
* **`DataService`**: `logic-tree.json`, `diagnostics.json`, `acupoints.json` 자산을 비동기로 호출하고 메모리 내에 캐싱 및 질의하는 데이터 제공 싱글톤 서비스.
* **`DiagnosisComponent (Smart)`**: 문진 진행 화면. 문진 상태 트래킹, 뒤로가기 스택(Array) 관리 및 결과 페이지로의 뷰 트랜지션 처리.
* **`AnatomyMapComponent (Interactive Guide)`**: 2D 인터랙티브 해부도 컴포넌트.
  * 처방된 혈자리가 속한 인체 부위(머리, 가슴/배, 등, 팔, 다리) 탭 버튼 자동 생성.
  * 선택된 부위의 고화질 해부학 지도 이미지를 백그라운드로 띄우고, 각 혈자리의 X/Y(%) 좌표에 펄스 애니메이션이 포함된 포인터(Pin) 배치.
* **`AcupointDetailComponent (Detail Panel)`**: 선택된 혈자리에 대한 마사지 상세 안내 가이드.
  * **세기(i) 시각화:** 강하게(Green, Deep Press, $i=1$) / 약하게(Red, Gentle Rub, $i=0$).
  * **방법(m) 시각화:** 누르기(Press, $m=1$), 문지르기(Rub, $m=2$), 두드리기(Tap, $m=3$)에 최적화된 CSS 키프레임 애니메이션 모듈 탑재.

---

## 4. UI/UX Design System (디자인 가이드라인)
* **Theme Palette (sleek Dark Mode):**
  * Background: `#09090b` (Deep Charcoal Black)
  * Card Background: `#121217` (Glassmorphism & Border line `#1e293b/80` 적용)
  * Accent Primary (Deep Press): `#10b981` (Emerald Green, $i=1$)
  * Accent Secondary (Gentle Rub): `#ef4444` (Vibrant Red, $i=0$)
* **Typography:** `Inter` 또는 `Outfit` 구글 폰트를 활용한 미니멀하고 가독성 높은 레이아웃.
* **Transitions:** 라우팅 또는 문진 카드 전환 시 `TranslateY` 슬라이드 및 `Fade-in` 애니메이션 적용으로 네이티브 앱 느낌 구현 (Motion 라이브러리 사용).

---

## 5. Build & Deployment Plan (빌드 및 배포 계획)

### PWA 배포 파이프라인
1. `@angular/pwa` 패키지를 추가하여 `ngsw-config.json` 정의.
2. 모든 JSON 자산 및 해부도 이미지 자산을 로컬 서비스 워커에 캐싱하여 완전 오프라인 구동 실현.
3. GitHub Pages 또는 Netlify 등을 활용해 정적 사이트로 배포.

### Capacitor 모바일 패키징
1. `@capacitor/core`, `@capacitor/cli` 및 안드로이드/iOS 플랫폼 패키지 추가.
2. `angular.json`의 빌드 출력 경로(`dist/app/browser`)를 Capacitor 설정(`capacitor.config.json`의 `webDir`)에 바인딩.
3. `npx cap sync` 명령어를 통해 네이티브 코드 동기화 후 Android Studio 또는 Xcode를 통해 네이티브 앱 빌드 수행.

---

## 6. 유지보수 AI 행동규칙
* Angular 21의 표준 문법과 RxJS 7+ 선언형 프로그래밍 방식을 엄격하게 적용할 것.
* 컴포넌트는 비즈니스 로직(Smart)과 순수 UI(Dumb) 역할을 명확히 구분할 것.
* CSS는 inline style을 지양하고, `TailwindCSS v4`의 유틸리티 클래스 및 CSS 변수 설계를 지향할 것.