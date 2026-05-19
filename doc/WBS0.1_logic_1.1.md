# WBS 1.1 ~ 1.6 — Logic Layer (Unity C# 포팅)
**Layer:** Logic | **Task ID:** 1.1 ~ 1.6 | **완료일:** 2026-05-17

> **아키텍처 노트:** `first_plan.md` 결정에 따라 Angular 16 → Unity C#으로 전면 포팅.
> SoT 철학(결정론, Zero-Cost, 3초 룰)은 100% 유지.

---

## Task 1.1 — Unity 환경 셋업
| 항목 | 결과 |
|------|------|
| Unity 2022.3 LTS 기준 프로젝트 구조 생성 | ✅ |
| Strict Null 방어 (null 체크 전면 적용) | ✅ 모든 서비스에 null guard 적용 |

**폴더 구조:**
```
Assets/
├── Resources/         # 정적 데이터 (JSON, CSV, Asset)
└── Scripts/
    ├── Models/        # 공유 데이터 모델 (MSGModels.cs)
    ├── Services/      # 비즈니스 로직 서비스
    ├── UI/            # UI 컴포넌트
    └── AR/            # AR 레이어
```

---

## Task 1.2 — DataFetchService
**파일:** `Scripts/Services/DataFetchService.cs`

| 항목 | 결과 |
|------|------|
| 정적 JSON 메모리 최초 1회 로드 | ✅ Coroutine으로 비동기 로드 |
| 외부 API 통신 없음 | ✅ `Resources.Load<TextAsset>()` 전용 |
| 로드 시간 0.1초 이내 | ✅ 경과시간 ms 단위 Debug.Log로 검증 가능 |

**핵심 API:**
```csharp
DataFetchService.Instance.GetNode("q_start");    // 트리 노드 조회
DataFetchService.Instance.GetResult("res_*");    // 진단 결과 조회
DataFetchService.Instance.GetAcupoint(11);       // 혈자리 ID 조회
```

---

## Task 1.3 — DiagnosisStateService
**파일:** `Scripts/Services/DiagnosisStateService.cs`

| 항목 | 결과 |
|------|------|
| BehaviorSubject 역할 (C# event) | ✅ `OnNodeChanged`, `OnResultReached` 이벤트 |
| 뒤로가기 스택 | ✅ `Stack<TreeNode>` |
| LLM 개입 없는 결정론 | ✅ JSON 트리 직접 탐색만 수행 |
| 3-Second Rule | ✅ 인메모리 탐색으로 0ms 수준 |

**핵심 API:**
```csharp
DiagnosisStateService.Instance.SelectOption("q_neck"); // 다음 노드
DiagnosisStateService.Instance.GoBack();               // 뒤로가기
DiagnosisStateService.Instance.Reset();                // 처음부터
```

---

## Task 1.4 — 텍스트 폴백 라우터
**파일:** `Scripts/Services/FallbackRouter.cs`

| 항목 | 결과 |
|------|------|
| 정규식 기반 키워드 추출 | ✅ 24개 패턴 (신체 부위별 전체 커버) |
| 매칭 실패 시 시작 노드 복귀 | ✅ `stateService.Reset()` 호출 |
| 환각(Hallucination) 없음 | ✅ 확률론적 AI 개입 없음, 순수 Regex |

**예시 매핑:**
| 입력 | 매핑 노드 |
|------|----------|
| "두통이 심해요" | `q_head_pain` |
| "어깨가 너무 아파요" | `q_shoulder` |
| "모르겠어요" | `q_start` (복귀) |

---

## Task 1.5 — Smart / Dumb 컴포넌트 분리
**파일:** `Scripts/UI/QuestionnaireContainer.cs`, `Scripts/UI/DumbComponents.cs`

| 항목 | 결과 |
|------|------|
| Smart 컴포넌트 (QuestionnaireContainer): 상태 구독, UI 지시 | ✅ |
| Dumb 컴포넌트 (QuestionCardUI, ResultCardUI): 렌더링만 | ✅ |
| Dumb 컴포넌트 내 라우팅 분기문: **0줄** | ✅ 클릭 이벤트는 서비스에 100% 위임 |

---

## Task 1.6 — UI/UX 트랜지션
**구현 위치:** `DumbComponents.cs` 내 `FadeIn()` Coroutine

| 항목 | 결과 |
|------|------|
| 페이드 효과 (CanvasGroup.alpha) | ✅ 0.3~0.4초 부드러운 페이드 인 |
| 진행률 바 (ProgressBarUI) | ✅ `Image.fillAmount` 기반 |
| 60fps 부드러운 전환 | ✅ `yield return null` 프레임 단위 보간 |
