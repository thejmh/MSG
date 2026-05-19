# [Project MSG] 단일 스마트폰 앱 구현을 위한 최적화 계획 (First Plan)

## 1. 현재 상황 분석 및 평가

### 1.1 설계 (SoT.md 기준)
- **2-Phase 아키텍처:** Angular 기반의 웹(진단)과 Unity 기반의 앱(AR 가이드)을 분리.
- **연결 매개체:** Base64로 인코딩된 JSON 데이터를 담은 **커스텀 딥링크(`msg-app://treat`)**.
- **철학:** 철저한 관심사 분리(SoC) 및 서버 비용이 없는(Zero-Cost) 완전 결정론적(Deterministic) 구조.

### 1.2 구현 소스
- **Angular (Phase 1):** `diagnosis.ts`(UI 및 라우팅), `handoff.ts`(딥링크 트리거) 초기 로직 구현됨.
- 앱 구조는 단순하며 외부 API 호출 없이 정적 JSON(`logic-tree.json`, `diagnostics.json`)만 메모리에 로드하여 작동 중.

### 1.3 데이터
- **`혈자리표.csv`:** 총 362줄 분량으로, 361개 혈자리의 ID, 경락, 명칭, 주요 증상, 위치 설명이 정리되어 있음.

---

## 2. 효율성과 심플함을 위한 아키텍처 재평가

사용자의 최종 목표인 **"가장 효율적이고 심플하게 스마트폰 앱으로 내보내는 것"**을 달성하기 위해 현재의 2-Phase (웹 -> 딥링크 -> 앱) 구조는 다음과 같은 오버헤드를 발생시킵니다.
1. 두 개의 분리된 프로젝트(Angular, Unity) 코드베이스 유지보수.
2. OS 레벨의 딥링크 설정 및 앱 간 전환 시 발생하는 UX 지연.
3. URI 길이 제한으로 인한 페이로드 확장성의 한계.

### 💡 제안하는 해결책: **Unity 단일 앱 통합 (Single Unity App)**
가장 심플하고 효율적인 방법은 **Angular로 구현된 진단(Logic) 파트를 Unity 앱 내부의 UI 씬(Scene)으로 통합**하는 것입니다.

* **SoT 철학의 유지:** 물리적인 앱은 하나로 합치되, 내부적으로 **[진단 씬]**과 **[AR 씬]**을 완벽히 분리합니다. 진단 씬은 AR 씬의 물리적 구조를 모르며, AR 씬은 진단 씬의 문진 맥락을 모른다는 기존의 **관심사 분리 원칙은 그대로 유지**할 수 있습니다.

---

## 3. 단계별 구현 마스터 플랜 (Action Plan)

### 단계 1: 데이터 통합 및 DB화 (Data Migration)
1. **혈자리 DB:** `data/혈자리표.csv`를 Unity의 `ScriptableObject` 리스트 또는 내장 SQLite 형태로 파싱하여 AR 씬에서 ID만으로 위치와 증상을 즉시 조회할 수 있게 구축합니다.
2. **결정 트리:** Angular에서 사용하던 `logic-tree.json`과 `diagnostics.json`을 Unity `Resources` 폴더로 이동시킵니다.

### 단계 2: Unity UI 기반 진단 로직 구현 (Logic Layer)
1. **UI 구축:** Unity의 Canvas (또는 UI Toolkit)를 사용하여 모바일 터치 친화적인 질문지 UI를 구축합니다.
2. **상태 관리:** Angular의 RxJS 상태 머신을 C# `event` 또는 UniRx 기반의 상태 머신으로 포팅하여 결정론적 라우팅을 구현합니다.
3. **씬 전환 (Handoff):** 딥링크(URI)를 생성하는 대신, `SceneManager.LoadScene`을 호출할 때 `dId`와 `pts` 배열(`id`, `i`, `m`) 정보만 Singleton 데이터 컨테이너를 통해 AR 씬으로 넘겨줍니다.

### 단계 3: AR 마사지 가이드 구현 (Physics Layer)
1. AR Foundation과 MediaPipe(Hand/Body)를 셋업합니다.
2. 전달받은 `pts` 배열의 혈자리 ID를 내장 DB에서 조회하여 해부학적 오프셋을 확인합니다.
3. 카메라 뎁스(Depth API)를 이용한 **동적 촌도 캘리브레이션**을 통해 사용자 개인의 생체 스케일에 맞춘 3D 마커를 렌더링합니다.

### 단계 4: 안드로이드 앱 빌드 및 최적화
1. Player Settings에서 불필요한 권한을 제거하고 오직 카메라(AR) 권한만 유지합니다.
2. 최적화 후 `.apk` 또는 `.aab` 형식으로 최종 안드로이드 스마트폰 앱을 빌드합니다.

---

## 4. 기대 효과
- **개발 공수 감소:** Angular 웹앱 개발 및 호스팅 설정 단계 삭제.
- **안정성 향상:** 딥링크를 통하지 않고 인메모리(In-memory)로 데이터를 안전하고 빠르게 넘김.
- **SoT 규칙 준수:** 결정론적 진단, LLM/외부 서버 배제 원칙, 데이터 Handoff 단방향 원칙 모두 충족.
