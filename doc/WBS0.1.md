### 📊 [통합 마스터플랜] 프로젝트 MSG v4.0 (Angular-AR 아키텍처)

| Phase (단계) | Task ID | Task 명 (작업명) | 세부 기술 스펙 (Technical Spec) | 완료 기준 (DoD / 6대 게이팅 검증) |
| --- | --- | --- | --- | --- |
| Phase 0:<br>

<br>스캐폴딩 및 데이터 | **0.1** | MSG 문진트리 JSON 정규화 | 구글 시트의 문진트리(DAG)를 `decision-tree.json`으로 변환. | ✅ Angular `src/assets/data/` 배치 완료.<br>

<br>⛔ DB 서버(API) 조회가 아님을 확인. |
|  | **0.2** | 혈자리표 V11 Unity DB화 | 총 361개 혈자리 메타데이터를 Unity `ScriptableObject` / SQLite로 포팅. | ✅ 외부 통신 없이 `id`만으로 해부학적 오프셋 즉시 반환 확인. |
| Phase 1:<br>

<br>The Logic Layer<br>

<br>(Angular) | **1.1** | Angular 환경 셋업 | Angular 16+ Workspace, Strict Type 검사, SCSS, RxJS 7+ 셋업. | ✅ `strictNullChecks: true` 컴파일 성공. |
|  | **1.2** | `DataFetchService` 구축 | `HttpClient`로 빌드 번들링된 정적 JSON 메모리 최초 1회 로드. | ✅ 로드 시간 0.1초 이내. 외부 API 통신 없음 확인. |
|  | **1.3** | `DiagnosisStateService` 구축 | `BehaviorSubject` 기반 노드 상태 및 뒤로가기 스택 관리. (결정론적 라우팅) | ✅ 3-Second Rule 준수. LLM 개입 없는 결정론 증명. |
|  | **1.4** | 텍스트 폴백 라우터 | 정규식(Regex) 기반 키워드 추출 및 트리의 특정 노드로 강제 맵핑. | ✅ 환각(Hallucination) 없이 매칭 실패 시 시작 노드로 복귀. |
|  | **1.5** | Smart / Dumb 컴포넌트 분리 | `QuestionnaireContainer`(상태)와 `QuestionCard`/`OptionGroup`(렌더링) 분리. | ✅ ⛔ Dumb 컴포넌트 내부에 라우팅 분기문이 0줄인지 확인. |
|  | **1.6** | UI/UX 트랜지션 적용 | 대형 카드 버튼, 진행률 바, Angular Animations 페이드 효과. | ✅ 화면 전환이 60fps로 부드럽게 이어짐. |
| Phase 2:<br>

<br>The Data Bridge<br>

<br>(Handoff) | **2.1** | 페이로드 Minification | 최종 리프 노드(`res_`) 데이터를 스키마(`dId`, `pts` 내 `id, i, m`)로 조립. | ✅ ⛔ X/Y/Z 좌표 등 불필요한 메타데이터 완벽 배제 확인. |
|  | **2.2** | Base64URL 직렬화 로직 | `HandoffService`. JSON 문자열화 및 RFC 4648 Base64URL 인코딩. | ✅ 특수문자/한글 포함 시에도 딥링크 변환 손실 없음. |
|  | **2.3** | 딥링크 UI 트리거 | "✨ AR 가이드 시작" 버튼에 `msg-app://treat?p=[Payload]` 바인딩. | ✅ Android OS에서 정상적으로 Intent 인터셉트 발생 확인. |
| Phase 3:<br>

<br>The Physics Layer<br>

<br>(Unity AR) | **3.1** | Unity AR/MediaPipe 셋업 | Unity 2022.3 LTS, AR Foundation, ARCore, MediaPipe Hand/Body 연동. | ✅ Android 기기 빌드 및 카메라 권한 획득 성공. |
|  | **3.2** | `DeepLinkReceiver` 구축 | `OnApplicationFocus`로 인텐트 감지, Base64 디코딩 후 C# 구조체 파싱. | ✅ ⛔ 웹의 문진 맥락 없이 순수 `id, i, m` 결과만 수신 확인. |
|  | **3.3** | 동적 촌도 캘리브레이션 | ARCore 뎁스로 두 랜드마크 3D 좌표 도출 ➔ 유클리드 거리 ➔ 1촌 절대 척도 계산. | ✅ 3D 공간 상 거리가 실제 신체 비례와 오차 5% 이내 산출. |
|  | **3.4** | 비선형 스킨 매핑 & 안정화 | 곡률 오차 보정 스플라인 적분, 1D 칼만 필터 및 IMU 자이로 틸트 보상. | ✅ 빠른 카메라 움직임에도 AR 마커 표류(Floating) 현상 없음. |
|  | **3.5** | `ARActionRenderer` 시각화 | 세기(`i`) 색상 렌더링, 방법(`m`) 홀로그램 분기, 경로(Pathline) 빛의 선 렌더링. | ✅ Strategy Pattern 적용으로 런타임 이펙트 정상 교체 확인. |
|  | **3.6** | 안착감 및 청각 피드백 | 가상 방향광 Fake Drop Shadow 투영 및 화면 밖 Spatial Audio 안내. | ✅ 무거운 광원 AI 없이 피부 밀착감 완벽 시뮬레이션 확인. |
| Phase 4:<br>

<br>Zero-Cost CI/CD | **4.1** | Phase 1 배포 (GitHub Pages) | Angular 프로젝트 정적 호스팅 자동 배포 파이프라인 구축. | ✅ 클라우드/서버 인프라 유지 비용 $0 확인. |
|  | **4.2** | Phase 2 Android APK 빌드 | Engine Code 최적화 및 `AndroidManifest.xml` 스킴 최종 검증 후 빌드. | ✅ 브라우저에서 딥링크 클릭 시 지연 없이 Unity 앱 전환 확인. |