# 🧠 [SoT 마스터 문서] 프로젝트 MSG (Meridian Symptom Guide) v4.0 - Hyper Full Spec

## 1. Core Specification (핵심 명세)
* **시스템 아이덴티티:** 사용자의 모호한 신체적 고통을 결정론적 논리망(DAG)으로 진단하고, 도출된 경락/혈자리 솔루션을 증강현실(AR) 공간에서 사용자 개인의 생체 스케일(동적 촌도)에 맞춰 3D로 표출하는 초개인화 무과금 의료 가이드 시스템.
* **플랫폼 아키텍처 (2-Phase 이원화):**
  * **Phase 1 (진단):** Angular 16+ 기반 반응형 웹 애플리케이션. (상태 비저장, 정적 JSON 트리기반 라우팅)
  * **Phase 2 (표출):** Unity 2022 LTS 기반 Android 모바일 AR 애플리케이션. (AR Foundation, ARCore, MediaPipe 연동)
* **데이터 백본 (Ground Truth):**
  * `결정 트리`: 'MSG 문진트리'의 정적 JSON 변환본.
  * `혈자리 DB`: '혈자리표 V11'에 정의된 총 361개 경락 메타데이터 (ID, 이름, 해부학적 위치, 증상, 중요도). Phase 2 앱 내부에 정적 자산으로 내장.

## 2. Project Concept & Philosophy (프로젝트 철학)
* **관심사의 완벽한 분리 (SoC):** 데이터의 성질이 '추상적 논리(웹)'에서 '구체적 물리(AR)'로 상전이(Phase Transition)하는 과정을 완벽히 디커플링. 두 시스템 간 결합도는 오직 '딥링크(Deep Link) URI' 하나로만 존재한다.
* **결정론적 구원 (Absolute Determinism):** 의료/건강 데이터의 특성상 확률론적 생성형 AI(LLM)의 환각(Hallucination) 개입을 원천 차단한다. 진단은 오로지 수학적으로 검증된 DAG(Directed Acyclic Graph) 트리를 통해서만 이루어진다.
* **Zero-Cost 인프라:** 클라우드 서버(AWS, Firebase 등) 유지 비용을 0원으로 수렴. Phase 1은 정적 호스팅(GitHub Pages 등)으로 해결하고, 통신은 기기 내장형 딥링크로 처리한다.
* **The 3-Second Rule:** 웹 UI의 노드 전환 및 반응은 3초(권장 0.1초) 이내에 처리되어야 한다. 네트워크 지연을 유발하는 구조는 설계 단계에서 기각한다.

## 3. High-Level Architecture (고수준 아키텍처)
* **[Phase 1] The Logic Layer (Angular Frontend):**
  * 사용자가 접속 시 `assets/decision-tree.json`을 메모리에 로드.
  * RxJS 기반 `DiagnosisStateService`가 현재 질문(Node) 상태 스트리밍.
  * 최종 리프 노드(`res_` 접두사) 도달 시, 처방 데이터를 경량화 JSON으로 조립.
* **[Data Bridge] Serialization & Deep Link Handoff:**
  * 조립된 JSON을 Base64URL 형식으로 인코딩하여 커스텀 URL 스킴 생성.
  * 형식: `msg-app://treat?p=[Base64_Encoded_JSON]`
* **[Phase 2] The Physics Layer (Unity AR Client):**
  * Android OS의 Intent Filter가 딥링크를 인터셉트하여 Unity 앱 호출.
  * `DeepLinkReceiver`가 Base64 파싱 후 `AcupointDB`와 매칭.
  * `DynamicCunCalibrator`가 MediaPipe와 ARCore Depth API를 활용해 1촌(Cun)의 절대 스케일을 산출하고 3D 마커 렌더링.

## 4. Current Implementation Features & Status (구현 현황)
* **[Phase 1] Angular 진단 시스템:** * 아키텍처 설계 및 컴포넌트 계층(Smart/Dumb) 분리 완료. (코드 구현 대기)
  * 다중 증상 복수 선택 처리 및 교집합 연산 로직 (구현 대기)
  * 텍스트 직접 입력 시 정규식 기반 폴백(Fallback) 라우터 (구현 대기)
* **[Data Bridge] 통신 규격:** 최소화 JSON 규격(`dId`, `pts` 배열 내 `id`, `i`, `m`) 및 Base64 핸드오프 설계 완료.
* **[Phase 2] AR 캘리브레이션 로직:** * 3D 뎁스 레이캐스팅 기반 유클리드 거리 산출 알고리즘 수학적 설계 완료.
  * 1D 칼만 필터(Kalman Filter) 및 자이로 틸트 보상 설계 완료. (코드 구현 대기)

## 5. Features Explicitly Considered and Dropped (폐기된 기획)
* **Telegram Bot & GAS 연동 (Phase 1):** 초창기 구현에 성공(CacheService 우회 등)했으나, 인라인 키보드의 `callback_data` 64바이트 제한, 잦은 웹훅 지연, 투박한 UI 한계로 인해 **Angular 웹앱으로 전면 마이그레이션 및 레거시 폐기**.
* **상태 저장을 위한 Backend DB (Firestore 등):** Zero-Cost 원칙에 정면 위배되므로 기각.
* **AR 페이로드에 3D 물리 좌표 직접 포함:** 딥링크 URL 길이 초과 유발 및 OS 보안 차단 위험으로 기각. (좌표는 Unity 내장 DB를 통해서만 매핑).
* **가상 광원(Light Estimation) AI 모델:** 연산 부하가 심해 기각. 대신 고전적 트릭인 **가상 그림자 투영(Fake Drop Shadow)** 기법으로 대체.

## 6. Technical Standard (기술 표준)
* **Phase 1:** Angular 16+, TypeScript 5+ (Strict Null Checks 필수), RxJS 7+, HTML5/SCSS.
* **Data Bridge:** RFC 4648 Base64URL 인코딩 (URL-Safe, 패딩 무시 허용).
* **Phase 2:** Unity 2022.3 LTS 이상, C# 9.0+, AR Foundation 5.0+, ARCore XR Plugin, MediaPipe Unity Plugin.

## 7. Storage & Path Strategy (데이터 저장 전략)
* **Phase 1 Routing Data:** `src/assets/data/decision-tree.json` (빌드 타임 번들링).
* **Phase 2 Anatomical DB:** Unity Project -> `Resources/AcupointDB.asset` (ScriptableObject) 또는 내장 SQLite.
* **State Persistence:** 앱 실행 중 RAM(RxJS BehaviorSubject)에만 의존하며, 브라우저 새로고침/종료 시 완전 초기화(Stateless).

## 8. Current Class Structure & Responsibilities (클래스 구조)
**[Angular Phase 1]**
* `DataFetchService`: 정적 JSON 트리 로드 (Pure Provider).
* `DiagnosisStateService`: 현재 노드 트래킹, 뒤로가기 스택(Array) 관리 (State Machine).
* `HandoffService`: 진단 결과 JSON 직렬화 및 `window.location.href = msg-app://...` 트리거.
* `QuestionnaireContainer (Smart)`: 상태 구독 및 하위 UI 렌더링 지시.
* `QuestionCard / OptionGroup (Dumb)`: `@Input()`으로 데이터 수신, `@Output()`으로 클릭 이벤트만 방출. 자체 상태(State) 절대 소유 금지.

**[Unity Phase 2]**
* `DeepLinkReceiver`: Android Intent 감지 (`OnApplicationFocus` 및 Unity Deep Linking API) 및 JSON 디시리얼라이즈.
* `DynamicCunCalibrator`: Camera 픽셀 -> 물리 단위 미터(m) 환산. 관절 간 거리 산출.
* `ARActionRenderer`: 페이로드의 `i`(Intensity)와 `m`(Method) 값에 따른 파티클 시스템 분기 처리.

## 9. Current UI / UX Details (UI/UX 디테일)
* **Phase 1 (Angular):** 모바일 터치 친화형 대형 카드, 부드러운 페이드 트랜지션, 현재 뎁스를 보여주는 Progress Bar.
* **Phase 2 (AR) 피드백 시스템:**
  * **마사지 세기 (Intensity, `i`):** `1` = 🟢 초록색 점 (강한 압박 Deep Press) / `0` = 🔴 빨간색 점 (살살 Gentle Rub).
  * **마사지 방법 (Method, `m`):** `1` = 누르기(Press), `2` = 문지르기(Rub), `3` = 두드리기(Tap). (투명 홀로그램 손 애니메이션으로 표출)
  * **경로(Pathline):** 배열 순서(Index)에 따라 혈자리를 잇는 빛나는 스플라인 렌더링.
  * **안착감 극대화:** 폰 자이로센서와 연동된 가상의 Drop Shadow 투영.
  * **청각 피드백:** Spatial Audio를 활용하여 화면 밖 혈자리 위치(Blind Lock-on) 청각 안내.

## 10. Current Data Schema Summary (데이터 스키마)
**[Data Bridge 통신용 Minified JSON 페이로드]**
```json
{
  "dId": "res_dig_01",       // 진단 노드 ID (Phase 2에서 타이틀 표출용)
  "pts": [                   // 치료해야 할 혈자리 배열 (순서 = 마사지 경로)
    {
      "id": 11,              // V11 혈자리 DB 기준 고유 ID (예: 합곡혈)
      "i": 1,                // 세기 (1: 초록/강하게, 0: 빨강/약하게)
      "m": 1                 // 방법 (1: Press, 2: Rub, 3: Tap)
    },
    { "id": 318, "i": 0, "m": 2 }
  ]
}

```

## 11. Design Patterns in Use (적용된 디자인 패턴)

* **프론트엔드 (Angular):** * `Smart / Dumb Component Pattern` (UI와 비즈니스 로직 완벽 격리)
* `Observer Pattern (RxJS)` (데이터 흐름에 따른 반응형 UI)


* **AR 클라이언트 (Unity):**
* `Strategy Pattern` (마사지 방법 `m`에 따른 렌더링 전략 런타임 교체)
* `Singleton Pattern` (AR Session Core, Device Camera Manager 단일 인스턴스 보장)



## 12. Algorithms & Behavior Rules (핵심 알고리즘)

* **역방향 검증(Reverse Verification) 알고리즘:** DAG 라우팅 중, 최종 리프 노드 도달 직전에 의도적으로 "숨을 쉴 때 쌕쌕거리는 소리가 심한가요?"와 같이 증상을 되묻는 노드(`R_L2_a` 등)를 배치하여 결정론적 신뢰도를 검증.
* **동적 촌도(Dynamic Cun) 산출 알고리즘:**
1. 단안 카메라 뎁스(ARCore Depth API)로 신체 랜드마크 $P_1(x_1, y_1, z_1)$ 및 $P_2(x_2, y_2, z_2)$ 미터(m) 좌표 획득.
2. 유클리드 거리 공식 적용: $D = \sqrt{(x_2 - x_1)^2 + (y_2 - y_1)^2 + (z_2 - z_1)^2}$
3. 신체 비례 규칙(예: 팔꿈치~손목 = 12촌)에 따라 $D / 12$ 수행하여 절대 1촌(m) 도출.


* **비선형 스킨 매핑 (Non-linear Skin Mapping):** 피부의 곡률(Contour)을 고려하여 직선 거리가 아닌 호의 길이(Arc Length) 적분식을 사용하여 마커 투영 오차 보정.

## 13. Real Code Reality Notes (실제 구현 시 주의사항)

* Base64 변환 시 `btoa()`, `atob()` 사용에 유의. 유니코드 문자가 포함될 경우 URIComponent 변환 후 인코딩해야 함 (URL Safe 처리).
* Android `AndroidManifest.xml` 내에 `<intent-filter>` 등록 시 `android:scheme="msg-app"` 및 `android:host="treat"` 설정을 정확히 기재해야 브라우저에서 Unity로 핸드오프됨.
* Unity AR Foundation 레이캐스팅 시, 평면(Plane)이 아닌 기하학적 뎁스(Depth Point) 맵핑을 우선시해야 사람의 피부 굴곡을 인식함.

## 14. Troubleshooting & Anti-Patterns (안티 패턴 및 트러블슈팅)

* **[Anti-Pattern] Angular Dumb 컴포넌트의 월권:** 버튼 클릭 시 다음 노드 ID를 컴포넌트 내부에서 계산하거나 서비스의 상태를 직접 변경하는 행위. 반드시 `EventEmitter`로 부모에게 위임해야 함.
* **[Anti-Pattern] 딥링크 비대화:** 페이로드에 불필요한 메타데이터(증상 문자열, 혈자리 X/Y/Z)를 담는 행위. 데이터 절단 및 딥링크 실행 실패의 원흉.
* **[Troubleshooting] 화면 밖 마커 증발:** MediaPipe 랜드마크 소실 시 AR 객체가 표류(Floating)하는 현상. C#에서 1D 칼만 필터(Kalman Filter)와 30프레임 측정 버퍼링을 적용해 위치를 보존해야 함.

## 15. Import Debug History (과거 디버그 히스토리 통합)

* **텔레그램 시대의 유산 (해결됨):**
* `callback_data` 64바이트 제한 ➔ 노드 ID 해싱/축약 기법 사용했으나 현재는 Angular 전환으로 제약 소멸.
* GAS `PropertiesService` 9KB 단일 페이로드 절단 이슈 ➔ `CacheService` 100KB 압축 동기화로 해결한 이력 있음. (현재는 통신 아예 제거)
* OS 딥링크 차단 이슈 ➔ 텔레그램 내 웹앱 트램펄린(HTML)을 띄워 브라우저 레벨에서 리다이렉트 하는 방식으로 우회한 경험 존재.



## 16. Current Constraints & Non-negotiable Rules (절대 제약 사항)

* **Rule 1 (결정론적 고립):** Angular Phase 1의 라우팅 결정망에 외부 API 호출이나 LLM 판단이 단 1byte라도 개입해서는 안 된다.
* **Rule 2 (Zero-Cost):** 어떠한 유료 DB(Firestore 등), 컨테이너(AWS EC2) 등도 아키텍처에 포함될 수 없다.
* **Rule 3 (단방향 격리):** 데이터는 `Angular ➔ Unity` 한 방향으로만 흐른다. Unity 앱은 사용자의 이전 문진 기록이나 대화 맥락을 절대 알 수 없고, 알 필요도 없다.

## 17. 유지보수 AI 행동규칙 (AI Behavior Protocol)

* **절대 컨텍스트:** 코드를 작성하기 전, 반드시 본 문서의 2-Phase 분리 원칙과 Zero-Cost 원칙을 복기할 것.
* **제안 통제:** 사용자가 "진단 결과를 DB에 저장하자" 또는 "GPT API를 붙이자"고 요구할 경우, 본 문서의 Rule 1, 2를 근거로 **강력히 거부**하고 무상태/결정론적 대안을 제시할 것.
* **코드 완결성:** 스니펫(Snippet) 형태의 파편화된 코드 제공을 금지한다. 즉시 Production 환경에 복사/붙여넣기 할 수 있는 Strict Type 적용 풀-코드를 작성할 것.

## 18. 향후 리팩토링/확장 시 절대 위반하면 안 되는 규칙

* **상태 침범 금지 (State Trespassing Prohibited):** Phase 1(웹)은 Phase 2(물리 공간)의 동적 촌도 계산 공식이나 3D 좌표를 알아서는 안 된다. Phase 2(Unity)는 Phase 1의 문진 과정(Symptom)을 몰라야 한다. 딥링크 JSON만이 유일한 컨트랙트(Contract)다.

## 19. Recommended Next Priorities (다음 우선순위)

1. **[Unity Editor]** `MSG > [SETUP] Full Project Setup (Run This First)` 메뉴 실행 → OptionButton 프리팹 생성 + 씬 자동 빌드.
2. **[Unity Editor]** `MSG > Parse Acupoints CSV` 실행 → `AcupointDB.asset` 생성.
3. **[Unity Editor]** AR Foundation 5.0+, ARCore XR Plugin 패키지 설치 (Package Manager).
4. **[Android Build]** File > Build Settings > Android 플랫폼 전환 후 빌드.
5. **[Phase 2 확장]** MediaPipe 자동 랜드마크 감지 도입 (수동 탭 → 자동 감지).

## 20. Implementation Status (구현 현황 — 최종 업데이트)

| 항목 | 상태 | 비고 |
|------|------|------|
| decision-tree.json (26개 결과 노드) | ✅ 완료 | |
| diagnostics.json (26개 처방 패키지) | ✅ 완료 | 혈자리 처방 데이터 완비 |
| AcupointCunOffset 테이블 | ✅ 완료 | 131개 등록, 처방 49개 전부 커버 |
| Unity 서비스 레이어 (C# 코드) | ✅ 완료 | |
| Unity UI 레이어 (C# 코드) | ✅ 완료 | |
| Unity AR 레이어 (C# 코드) | ✅ 완료 | |
| SceneBuilder 에디터 스크립트 | ✅ 완료 | MSG > Build All Scenes |
| PrefabBuilder 에디터 스크립트 | ✅ 완료 | MSG > Build OptionButton Prefab |
| AndroidManifest.xml | ✅ 완료 | msg-app:// 딥링크 + 카메라 권한 |
| Unity 씬 파일 와이어링 | ⏳ 대기 | Editor에서 MSG > [SETUP] 실행 필요 |
| AR Foundation 패키지 설치 | ⏳ 대기 | Package Manager에서 수동 설치 |
| 실기기 빌드 테스트 | ⏳ 대기 | |

## 20. Final Principle (궁극의 본질과 방어선)

**[본질 요약]**

> 프로젝트 MSG는 백엔드 비용이 전혀 들지 않는 무상태 Angular 웹 문진표와 초개인화 Unity AR 앱을 딥링크로 완벽하게 디커플링한, 환각(Hallucination) 없는 결정론적 의료 가이드 시스템이다.

**[아키텍처 수호 6대 게이팅 질문 (Gating Questions)]**
*(아래 질문 중 하나라도 'Yes'가 도출되면, 즉시 코드 구현을 중단하고 설계(Architecture)를 롤백(Roll-back)한다.)*

1. 이 기능이 서버 유지보수 비용이나 외부 API 통신 비용을 발생시키는가?
2. 확률적 AI(LLM)가 문진 결과 도출이나 의료적 판단 라우팅에 개입할 여지가 존재하는가?
3. 딥링크 페이로드에 Unity 내장 DB에서 이미 참조할 수 있는 데이터(예: 혈자리 이름, 3D 좌표)가 중복 포함되어 URL 길이가 불필요하게 커졌는가?
4. Angular의 Dumb 컴포넌트가 다음 라우팅 노드 ID를 직접 연산하거나 RxJS 상태를 임의로 수정하는가?
5. Unity(Phase 2) 앱이 AR 렌더링을 위해 사용자의 증상(Symptom) 맥락 데이터를 요구하는가?
6. 사용자가 옵션 버튼을 터치한 후 다음 문진 카드로 트랜지션 되기까지 3초 이상의 네트워크/연산 딜레이가 발생하는가?