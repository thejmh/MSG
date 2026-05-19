# Unity 씬 설정 가이드 (초보자용)
**Project MSG — 진단 씬 & AR 씬 셋업**

> 이 문서는 Unity를 처음 사용하는 분도 따라할 수 있도록 모든 단계를 스크린샷 위치까지 설명합니다.

---

## 📌 Unity 화면 구성 먼저 이해하기

```
┌──────────────────────────────────────────────────────────┐
│  메뉴바: File / Edit / Assets / GameObject / Window ...   │
├────────────┬────────────────────────┬────────────────────┤
│            │                        │                    │
│ Hierarchy  │     Scene View         │   Inspector        │
│ (계층구조)  │   (3D 작업 공간)        │  (속성 패널)       │
│            │                        │                    │
│            ├────────────────────────┤                    │
│            │     Game View          │                    │
│            │   (실제 게임 화면)       │                    │
├────────────┴────────────────────────┴────────────────────┤
│  Project (Assets 파일 탐색기)  │  Console (로그)          │
└──────────────────────────────────────────────────────────┘
```

| 패널 이름 | 역할 | 비유 |
|:---|:---|:---|
| **Hierarchy** | 현재 씬에 있는 오브젝트 목록 | 파일 탐색기의 폴더 트리 |
| **Inspector** | 선택한 오브젝트의 속성/컴포넌트 | 속성 창 |
| **Project** | Assets 폴더의 모든 파일 | 탐색기 |
| **Scene View** | 3D 작업 공간 | 무대 위 |
| **Game View** | 실제 실행 화면 | 관객 시점 |

---

## 🏗️ STEP 0: 현재 상태 이해하기

지금 Hierarchy에 보이는 것:
```
Untitled        ← 이게 씬(Scene)의 이름입니다
  └─ Main Camera
  └─ Directional Light
```

> **씬(Scene)** = 하나의 "무대"입니다. 게임은 여러 씬이 순서대로 실행됩니다.
> `Untitled`는 아직 저장되지 않은 빈 씬입니다.

---

## 🗂️ STEP 1: 혈자리 DB 에셋 생성 (제일 먼저!)

> ⚠️ 씬 작업 전에 반드시 먼저 실행해야 합니다.

1. Unity 상단 메뉴에서 **MSG → Parse Acupoints CSV** 클릭
2. Console 패널에 아래 메시지가 뜨면 성공:
   ```
   AcupointDB successfully created with 361 entries.
   ```
3. **Project 패널** → `Assets/Resources` 폴더에 `AcupointDB.asset` 파일이 생겼는지 확인

---

## 🧩 STEP 2: 진단 씬(DiagnosisScene) 만들기

### 2-1. 현재 씬을 "진단 씬"으로 저장

1. 메뉴 **File → Save As...**
2. 파일 이름: `DiagnosisScene`
3. 저장 위치: `Assets/Scenes/` 폴더 (없으면 새로 만들기)
4. 저장 후 Hierarchy 상단이 `DiagnosisScene`으로 바뀌면 성공

### 2-2. 서비스 매니저 오브젝트 만들기

> 이 오브젝트는 "눈에 안 보이는 비즈니스 로직 담당 서버"입니다.

1. Hierarchy 빈 곳 **우클릭 → Create Empty**
2. 이름을 `[ServiceManager]` 로 변경
   - 오브젝트 클릭 후 **F2** 키 또는 Inspector 상단 이름 필드 클릭
3. `[ServiceManager]` 오브젝트가 선택된 상태에서 **Inspector 패널** 아래쪽의 **Add Component** 버튼 클릭
4. 검색창에 아래 스크립트들을 **하나씩** 검색하여 추가:

| 검색어 | 추가할 스크립트 |
|:---|:---|
| `DataFetchService` | ✅ 클릭하여 추가 |
| `DiagnosisStateService` | ✅ 클릭하여 추가 |
| `FallbackRouter` | ✅ 클릭하여 추가 |
| `HandoffService` | ✅ 클릭하여 추가 |
| `HandoffData` | ✅ 클릭하여 추가 |

> 💡 Add Component 후 Inspector에 5개의 컴포넌트 블록이 쌓이면 정상입니다.

### 2-3. Canvas 만들기

> Canvas = UI 요소들이 그려지는 "도화지"입니다.

1. Hierarchy 빈 곳 **우클릭 → UI → Canvas**
2. 자동으로 `Canvas`와 `EventSystem`이 생성됩니다 (정상)
3. Canvas 선택 → Inspector에서 **Canvas Scaler** 컴포넌트 찾기
   - `UI Scale Mode` → **Scale With Screen Size** 로 변경
   - `Reference Resolution` → `X: 1080, Y: 1920` (세로 스마트폰 기준)

### 2-4. Canvas 안에 UI 구조 만들기

> Canvas를 **우클릭**하여 하위 오브젝트를 만들어야 합니다.

```
Canvas
├── [QuestionnaireContainer]   ← 빈 오브젝트 (스크립트 담당)
├── LoadingPanel               ← UI > Panel
├── QuestionCard               ← UI > Panel
│   ├── QuestionText           ← UI > Text - TextMeshPro  (또는 Text)
│   └── OptionsContainer       ← 빈 오브젝트
├── ResultCard                 ← UI > Panel
│   ├── TitleText              ← UI > Text
│   ├── SubtitleText           ← UI > Text
│   ├── LaunchARButton         ← UI > Button
│   └── ResetButton            ← UI > Button
├── BackButton                 ← UI > Button
└── ProgressBar                ← UI > Slider (또는 빈 오브젝트)
    └── Fill                   ← UI > Image
```

**만드는 순서:**

① Canvas 우클릭 → **Create Empty** → 이름: `[QuestionnaireContainer]`

② Canvas 우클릭 → **UI → Panel** → 이름: `LoadingPanel`
- Inspector → Image 컴포넌트 Color: 검정색, Alpha 180 정도

③ Canvas 우클릭 → **UI → Panel** → 이름: `QuestionCard`
- `QuestionCard` 우클릭 → **UI → Text** → 이름: `QuestionText`
- `QuestionCard` 우클릭 → **Create Empty** → 이름: `OptionsContainer`
  - `OptionsContainer` Inspector → **Add Component → Vertical Layout Group** 추가

④ Canvas 우클릭 → **UI → Panel** → 이름: `ResultCard`
- `ResultCard` 우클릭 → **UI → Text** → 이름: `TitleText`
- `ResultCard` 우클릭 → **UI → Text** → 이름: `SubtitleText`
- `ResultCard` 우클릭 → **UI → Button** → 이름: `LaunchARButton`
  - 하위 `Text` 선택 → Inspector Text 내용을 `✨ AR 가이드 시작` 으로 변경
- `ResultCard` 우클릭 → **UI → Button** → 이름: `ResetButton`
  - 하위 `Text` → `다시 진단하기`

⑤ Canvas 우클릭 → **UI → Button** → 이름: `BackButton`
- 하위 `Text` → `← 이전`

⑥ Canvas 우클릭 → **Create Empty** → 이름: `ProgressBar`
- `ProgressBar` 우클릭 → **UI → Image** → 이름: `Fill`

### 2-5. 옵션 버튼 프리팹 만들기

> 프리팹 = 재사용 가능한 오브젝트 템플릿입니다.

1. Hierarchy에서 Canvas 우클릭 → **UI → Button** → 이름: `OptionButton`
2. 버튼의 크기/스타일을 원하는 대로 설정 (Inspector의 RectTransform으로 크기 조정)
3. `OptionButton` 을 **Project 패널의 Assets 폴더로 드래그** → 프리팹으로 저장됨 (파란색 아이콘으로 변경)
4. Hierarchy의 `OptionButton`은 **Delete** 키로 삭제 (프리팹은 Project 패널에 남아있음)

### 2-6. 자식 오브젝트에 스크립트 먼저 붙이기 ⚠️ 이 단계가 먼저!

> **중요:** QuestionnaireContainer의 Inspector 필드 타입이 `QuestionCardUI`, `ResultCardUI` 등 **특정 컴포넌트 타입**입니다.
> 해당 컴포넌트가 오브젝트에 **먼저 붙어있어야** 드래그 연결이 됩니다.

**① QuestionCard 오브젝트에 스크립트 추가**
1. Hierarchy에서 `QuestionCard` 선택
2. Inspector → **Add Component → QuestionCardUI** 검색 후 추가
3. 아래 필드 연결:

| Inspector 필드 | 연결할 오브젝트 |
|:---|:---|
| `Question Text` | `QuestionCard` 하위의 `QuestionText` |
| `Options Container` | `QuestionCard` 하위의 `OptionsContainer` |
| `Option Button Prefab` | Project 패널의 `OptionButton` **프리팹** |

**② ResultCard 오브젝트에 스크립트 추가**
1. Hierarchy에서 `ResultCard` 선택
2. Inspector → **Add Component → ResultCardUI** 검색 후 추가
3. 아래 필드 연결:

| Inspector 필드 | 연결할 오브젝트 |
|:---|:---|
| `Title Text` | `TitleText` |
| `Subtitle Text` | `SubtitleText` |
| `Launch A R Button` | `LaunchARButton` |
| `Reset Button` | `ResetButton` |

**③ LoadingPanel에 스크립트 추가**
1. `LoadingPanel` 선택
2. **Add Component → LoadingUI** 추가 (필드 연결 없음)

**④ ProgressBar에 스크립트 추가**
1. `ProgressBar` 선택
2. **Add Component → ProgressBarUI** 추가
3. `Fill Image` 필드 → `ProgressBar` 하위 of `Fill` 이미지 드래그 연결

---

### 2-7. QuestionnaireContainer 스크립트 연결 (이제 가능!)

> 이전 단계(2-6)에서 모든 컴포넌트를 먼저 붙였기 때문에 이제 드래그가 됩니다.

1. Hierarchy에서 `[QuestionnaireContainer]` 오브젝트 선택
2. Inspector → **Add Component → QuestionnaireContainer** 추가
3. 아래 필드에 Hierarchy 오브젝트를 드래그:

| Inspector 필드 | Hierarchy에서 연결할 오브젝트 | 타입 |
|:---|:---|:---|
| `Question Card` | `QuestionCard` | QuestionCardUI |
| `Result Card` | `ResultCard` | ResultCardUI |
| `Loading UI` | `LoadingPanel` | LoadingUI |
| `Back Button` | `BackButton` | Button |
| `Progress Bar` | `ProgressBar` | ProgressBarUI |

> 💡 **드래그가 안 될 때 체크리스트:**
> - 해당 오브젝트에 대응하는 컴포넌트가 붙어 있는가? (2-6 완료 여부)
> - Unity 하단에 로딩 스피너가 돌고 있지 않은가? (컴파일 중에는 안 됨)
> - Inspector 상단 자물쇠(🔒) 아이콘이 잠겨 있지 않은가?
| `Reset Button` | `ResetButton` |

---

### ⚡ 2-8. UI 1초만에 100% 자동 완벽 정렬하기 (코드로 해결!) 🚀

> 마우스로 크기나 정렬을 힘들게 직접 맞출 필요가 전혀 없습니다. 자동 정렬 스크립트를 사용해 황금 비율로 최적화합니다.

1. Hierarchy에서 최상단의 **Canvas** 오브젝트 선택
2. Inspector 패널 맨 아래의 **Add Component** 클릭 → `UILayoutOptimizer` 검색 후 추가
3. Inspector에서 `UILayoutOptimizer` 컴포넌트 이름 영역을 **우클릭** (혹은 우측 톱니바퀴 ⚙️ 아이콘 클릭)
4. 메뉴에서 **✨ UI 즉시 자동 정렬 및 최적화** (또는 Optimize Layout) 클릭!
5. **결과:** 그 즉시 모든 패널, 텍스트, 여백, 모던한 카드 그림자 효과(Shadow)까지 **황금 세로형 스마트폰 비율**에 맞춰 마법처럼 재정렬됩니다.

> 💡 이 스크립트는 게임을 실행(▶ Play)할 때도 모든 화면 비율에 맞춰 자동으로 한 번 더 정렬해주기 때문에, 디자인이 절대 깨지지 않습니다.

---

### 2-9. 초기 상태 설정

게임 시작 시 `QuestionCard`와 `ResultCard`는 숨겨져 있어야 합니다.
1. `QuestionCard` 선택 → Inspector 상단의 체크박스(오브젝트 이름 옆) **해제** (비활성화)
2. `ResultCard` 선택 → 동일하게 **해제**
3. `LoadingPanel`은 활성화 상태 유지

### 2-10. 진단 씬 저장

- **Ctrl + S** 로 저장

---

## 🔮 STEP 3: AR 씬(ARScene) 만들기

### 3-1. 새 씬 생성

1. 메뉴 **File → New Scene**
2. **Basic (Built-in)** 선택 → Create
3. 메뉴 **File → Save As...** → 이름: `ARScene`, 위치: `Assets/Scenes/`

### 3-2. AR Foundation 패키지 설치 (최초 1회)

> ⚠️ 이 단계를 건너뛰면 AR 스크립트들이 빨간 에러를 표시합니다.

1. 메뉴 **Window → Package Manager**
2. 좌상단 드롭다운을 **Unity Registry** 로 변경
3. 검색창에 `AR Foundation` 입력 → **AR Foundation** 선택 → **Install**
4. 검색창에 `ARCore` 입력 → **ARCore XR Plugin** 선택 → **Install**
5. 설치 완료 후 Package Manager 닫기

### 3-3. AR Session Origin 설정

> AR 씬의 기본 구조를 만듭니다.

1. Hierarchy의 `Main Camera`와 `Directional Light` 선택 후 **Delete**
2. Hierarchy 빈 곳 우클릭 → **XR → AR Session**
3. Hierarchy 빈 곳 우클릭 → **XR → AR Session Origin** (또는 XR Origin)

> 💡 `AR Session Origin` 안에 `AR Camera`가 자동으로 생성됩니다.

### 3-4. AR Camera 컴포넌트 추가

1. Hierarchy에서 `AR Session Origin` → `AR Camera` 선택
2. Inspector → **Add Component → AR Camera Manager** 추가
3. Inspector → **Add Component → AR Occlusion Manager** 추가
4. Inspector → **Add Component → AR Raycast Manager** 추가 ⬅️ **필수! (혈자리 위치 계산용)**

> ⚠️ **AR Raycast Manager는 혈자리 위치를 실제 3D 공간에 앵커링하기 위해 반드시 필요합니다.**

### 3-5. AR 서비스 매니저 오브젝트 만들기

1. Hierarchy 빈 곳 우클릭 → **Create Empty** → 이름: `[ARManager]`
2. `[ARManager]` 선택 → Inspector에서 아래 스크립트 **하나씩 추가**:

| 검색어 | 추가할 스크립트 |
|:---|:---|
| `DeepLinkReceiver` | ✅ 추가 |
| `DynamicCunCalibrator` | ✅ 추가 |
| `ARActionRenderer` | ✅ 추가 |
| `ARSceneController` | ✅ 추가 |

### 3-6. ARSceneController 필드 연결

`[ARManager]` 가 선택된 상태에서 Inspector의 **ARSceneController** 컴포넌트를 찾아 연결:

| Inspector 필드 | 연결할 오브젝트 |
|:---|:---|
| `Ar Session` | Hierarchy의 `AR Session` |
| `Ar Camera Manager` | `AR Session Origin/AR Camera` |
| `Ar Raycast Manager` | `AR Session Origin/AR Camera` ⬅️ **신규** |
| `Cun Calibrator` | `[ARManager]` (자기 자신) |
| `Action Renderer` | `[ARManager]` (자기 자신) |
| `Deep Link Receiver` | `[ARManager]` (자기 자신) |

> 💡 **AR Raycast Manager**: `AR Camera` 오브젝트를 필드에 드래그합니다. 이 컴포넌트가 없으면 실제 기기에서 터치로 혈자리 위치를 지정할 수 없습니다.

### 3-7. AR 씬 저장

- **Ctrl + S** 로 저장

---

## 📋 STEP 4: Build Settings에 씬 등록

> Unity는 빌드에 포함될 씬을 명시적으로 등록해야 합니다.

1. 메뉴 **File → Build Settings**
2. **Add Open Scenes** 버튼 클릭 (현재 열린 씬이 추가됨)
3. `DiagnosisScene`도 추가하려면: Project 패널에서 `DiagnosisScene`을 **Scenes In Build** 목록에 드래그
4. 최종 목록 확인:

```
Scenes In Build:
  [0] Scenes/DiagnosisScene   ← 반드시 0번!
  [1] Scenes/ARScene
```

> ⚠️ **순서가 매우 중요합니다.** `DiagnosisScene`이 반드시 0번이어야 합니다.
> 드래그하여 순서 변경 가능합니다.

---

## 📱 STEP 5: Android 빌드 설정

1. Build Settings에서 Platform 목록 중 **Android** 선택 → **Switch Platform** 클릭
   (시간이 걸릴 수 있습니다, 기다리세요)
2. **Player Settings** 버튼 클릭 → 아래 항목 설정:

| 설정 항목 | 값 |
|:---|:---|
| Company Name | 원하는 이름 |
| Product Name | MSG |
| Minimum API Level | Android 7.0 (API 24) |

3. **Other Settings** 탭:
   - `Auto Graphics API` **체크 해제** → OpenGLES3 추가
   - `Scripting Backend` → **IL2CPP**
   - `Target Architectures` → **ARM64** 체크

---

## ▶️ STEP 6: 에디터에서 테스트 실행

AR 없이 진단 씬만 먼저 테스트할 수 있습니다:

1. `DiagnosisScene` 열기 (Project 패널 → Scenes → `DiagnosisScene` 더블클릭)
2. Hierarchy에서 오브젝트들이 보이는지 확인
3. 상단의 **▶ Play 버튼** 클릭
4. Game View 탭으로 자동 전환됨
5. Console 패널에서 아래 로그 확인:
   ```
   [DataFetchService] decision-tree.json 로드 완료: 36개 노드
   [DataFetchService] diagnostics.json 로드 완료: 26개 패키지
   [DataFetchService] 전체 로드 완료 (XX.Xms) - 3초 룰 준수: True
   [DiagnosisStateService] 현재 노드: q_start
   ```
6. **▶ Play 버튼** 다시 클릭하여 종료

---

## 🛠️ 자주 하는 실수 & 해결법

| 문제 | 원인 | 해결 |
|:---|:---|:---|
| Console에 빨간 에러가 없는데 게임이 안 됨 | Play 모드가 아님 | ▶ Play 버튼 클릭 |
| `NullReferenceException` 에러 | Inspector 필드가 연결 안 됨 | 해당 스크립트의 필드 확인 및 드래그 연결 |
| `decision-tree.json` 못 찾음 | Resources 폴더에 파일 없음 | Project 패널 → Resources 폴더 확인 |
| `AcupointDB.asset` 못 찾음 | 파싱 스크립트 미실행 | **MSG → Parse Acupoints CSV** 실행 |
| AR 스크립트 에러 | AR Foundation 미설치 | Package Manager에서 설치 |
| 씬 전환이 안 됨 | Build Settings 씬 미등록 | File → Build Settings → Add Open Scenes |
| Play 중 화면이 검음 | QuestionCard/ResultCard 비활성화됨 | LoadingPanel이 활성화 상태인지 확인 |

---

## 📁 최종 Hierarchy 구조 참조

### DiagnosisScene
```
DiagnosisScene
├── [ServiceManager]
│   ├── DataFetchService (Script)
│   ├── DiagnosisStateService (Script)
│   ├── FallbackRouter (Script)
│   ├── HandoffService (Script)
│   └── HandoffData (Script)
├── Canvas
│   ├── [QuestionnaireContainer]
│   │   └── QuestionnaireContainer (Script)
│   ├── LoadingPanel (활성화)
│   ├── QuestionCard (비활성화)
│   │   ├── QuestionText
│   │   ├── OptionsContainer
│   │   └── QuestionCardUI (Script)
│   ├── ResultCard (비활성화)
│   │   ├── TitleText
│   │   ├── SubtitleText
│   │   ├── LaunchARButton
│   │   ├── ResetButton
│   │   └── ResultCardUI (Script)
│   ├── BackButton
│   └── ProgressBar
│       └── Fill (Image)
└── EventSystem
```

### ARScene
```
ARScene
├── AR Session
├── AR Session Origin
│   └── AR Camera
│       ├── ARCameraManager (Component)
│       └── AROcclusionManager (Component)
└── [ARManager]
    ├── DeepLinkReceiver (Script)
    ├── DynamicCunCalibrator (Script)
    ├── ARActionRenderer (Script)
    └── ARSceneController (Script)
```
