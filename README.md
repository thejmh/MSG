<div align="center">

<br/>

```
███╗   ███╗███████╗ ██████╗
████╗ ████║██╔════╝██╔════╝
██╔████╔██║███████╗██║  ███╗
██║╚██╔╝██║╚════██║██║   ██║
██║ ╚═╝ ██║███████║╚██████╔╝
╚═╝     ╚═╝╚══════╝ ╚═════╝
```

**Meridian Symptom Guide**

*당신의 고통을 결정론적 논리로 진단하고, AR 공간에서 당신의 몸에 맞게 안내합니다*

<br/>

[![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com)
[![AR Foundation](https://img.shields.io/badge/AR_Foundation-5.0+-FF6B6B?style=for-the-badge&logo=unity&logoColor=white)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0)
[![Android](https://img.shields.io/badge/Android-API_24+-3DDC84?style=for-the-badge&logo=android&logoColor=white)](https://developer.android.com)
[![License](https://img.shields.io/badge/License-MIT-4ECDC4?style=for-the-badge)](LICENSE)
[![Zero Cost](https://img.shields.io/badge/Infra_Cost-$0-FFE66D?style=for-the-badge&logo=serverless&logoColor=black)](https://github.com)

<br/>

</div>

---

<br/>

## 🧠 프로젝트 소개

> **"사용자의 모호한 신체적 고통을 결정론적 논리망으로 진단하고,  
> 도출된 혈자리 솔루션을 증강현실(AR) 공간에서  
> 사용자 개인의 생체 스케일(동적 촌도)에 맞춰 3D로 표출하는  
> 초개인화 무과금 의료 가이드 시스템"**

프로젝트 MSG는 **확률론적 AI(LLM)의 환각(Hallucination)을 원천 차단**하고,  
순수한 **수학적 결정 트리(DAG)**만으로 경락/혈자리 마사지를 안내합니다.  
서버 인프라 비용은 **$0**입니다.

<br/>

---

## ✨ 핵심 특징

<div align="center">

|  | 특징 | 설명 |
|:---:|:---|:---|
| 🧬 | **결정론적 진단** | LLM 없음. 수학적 DAG 트리만으로 100% 결정론적 라우팅 |
| 📱 | **단일 스마트폰 앱** | Unity 하나로 진단 + AR 가이드까지 완결 |
| 💰 | **Zero-Cost 인프라** | 서버 없음, API 비용 없음, 정적 에셋만 사용 |
| 🎯 | **초개인화 AR** | 사용자 신체 비례(동적 촌도)에 맞춘 3D 혈자리 마킹 |
| 🏃 | **3초 룰** | 모든 화면 전환이 네트워크 없이 0.1초 이내 처리 |
| 🔒 | **완전 분리** | 진단 로직 ↔ AR 표출이 단방향 데이터로만 연결 |

</div>

<br/>

---

## 🏗️ 아키텍처

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PROJECT MSG — Single Unity App                    │
│                                                                       │
│  ┌──────────────────────┐          ┌──────────────────────────────┐  │
│  │   🧩 DIAGNOSIS SCENE │          │      🔮 AR GUIDE SCENE       │  │
│  │   (Logic Layer)       │          │      (Physics Layer)          │  │
│  │                       │  In-     │                              │  │
│  │  decision-tree.json   │  Memory  │  DynamicCunCalibrator        │  │
│  │       ↓               │ ──────▶  │  (유클리드 거리 → 1촌 척도)   │  │
│  │  DiagnosisStateService│  Handoff │       ↓                      │  │
│  │  (결정론적 DAG 탐색)   │  Payload │  ARActionRenderer            │  │
│  │       ↓               │          │  (Strategy Pattern)           │  │
│  │  HandoffService       │          │  • i=1 🟢 강하게 Press        │  │
│  │  (Payload 조립)       │          │  • i=0 🔴 약하게 Rub/Tap      │  │
│  └──────────────────────┘          │  • Pathline 빛의 선 렌더링    │  │
│                                     └──────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘

    정적 데이터 (외부 통신 없음)
    ┌─────────────────┐  ┌──────────────────┐  ┌───────────────────┐
    │ decision-tree   │  │  diagnostics     │  │  AcupointDB       │
    │    .json        │  │     .json        │  │ (361개 혈자리)    │
    │  (10 Q-노드     │  │ (26가지 증상별   │  │  ScriptableObject)│
    │  26 결과 리프)  │  │  마사지 패키지)  │  │                   │
    └─────────────────┘  └──────────────────┘  └───────────────────┘
```

<br/>

---

## 📋 데이터 플로우

```
사용자 증상 입력
       │
       ▼
  ┌─────────┐   결정론적 탐색    ┌──────────┐
  │  DAG    │ ───────────────▶ │  결과    │
  │  트리   │   (LLM 없음)      │  리프    │
  └─────────┘                   └────┬─────┘
                                      │
                              페이로드 조립
                              {dId, pts:[{id,i,m}]}
                              (X/Y/Z 좌표 배제)
                                      │
                              ┌───────▼──────┐
                              │  AR 씬 전환  │
                              │  (In-Memory) │
                              └───────┬──────┘
                                      │
                              ┌───────▼───────────┐
                              │  동적 촌도 계산    │
                              │  D = √(Δx²+Δy²+Δz²)│
                              │  1촌 = D / 12      │
                              │  (칼만 필터 안정화) │
                              └───────┬────────────┘
                                      │
                              ┌───────▼──────────┐
                              │  AR 마커 렌더링   │
                              │  + 경로선(Pathline)│
                              │  + Spatial Audio  │
                              └──────────────────┘
```

<br/>

---

## 🗂️ 프로젝트 구조

```
MSG/
├── 📁 UnityProject/
│   └── Assets/
│       ├── 📁 Resources/              # 정적 에셋 (빌드 번들링)
│       │   ├── decision-tree.json     # 문진 DAG 트리 (10Q + 26 리프)
│       │   ├── diagnostics.json       # 26가지 증상별 마사지 패키지
│       │   ├── Acupoints.csv          # 혈자리표 V11 (361개)
│       │   └── AcupointDB.asset       # ← Editor 파싱 후 생성
│       └── Scripts/
│           ├── 📁 Models/
│           │   └── MSGModels.cs       # 전체 공유 데이터 모델
│           ├── 📁 Services/           # 비즈니스 로직
│           │   ├── DataFetchService.cs      # [WBS 1.2] 정적 JSON 로더
│           │   ├── DiagnosisStateService.cs # [WBS 1.3] 상태 머신
│           │   ├── FallbackRouter.cs        # [WBS 1.4] Regex 폴백 라우터
│           │   └── HandoffService.cs        # [WBS 2.1~2.3] 페이로드 & 씬 전환
│           ├── 📁 UI/                 # UI 컴포넌트
│           │   ├── QuestionnaireContainer.cs # [WBS 1.5] Smart Component
│           │   └── DumbComponents.cs         # [WBS 1.5/1.6] Dumb Components
│           ├── 📁 AR/                 # AR 레이어
│           │   ├── ARSceneController.cs     # [WBS 3.1/3.6] AR 씬 컨트롤러
│           │   ├── DeepLinkReceiver.cs      # [WBS 3.2] 페이로드 수신
│           │   ├── DynamicCunCalibrator.cs  # [WBS 3.3/3.4] 촌도 + 칼만필터
│           │   └── ARActionRenderer.cs      # [WBS 3.5/3.6] Strategy 렌더러
│           └── 📁 Data/
│               ├── AcupointDB.cs      # ScriptableObject 컨테이너
│               ├── CSVParser.cs       # 에디터 자동 파서 (MSG > Parse CSV)
│               └── HandoffData.cs     # DontDestroyOnLoad 씬 간 데이터
│
├── 📁 data/
│   └── 혈자리표.csv                   # V11 원본 데이터 (361개)
│
└── 📁 doc/
    ├── SoT.md                         # Source of Truth (아키텍처 헌법)
    ├── WBS0.1.md                      # 통합 마스터플랜
    ├── first_plan.md                  # Unity 단일 앱 결정 문서
    ├── WBS0.1_data_0.1.md             # ✅ 문진트리 JSON 정규화 완료
    ├── WBS0.1_data_0.2.md             # ✅ 혈자리표 DB화 완료
    ├── WBS0.1_logic_1.1.md            # ✅ Logic Layer (1.1~1.6) 완료
    ├── WBS0.1_data_2.1.md             # ✅ Data Bridge (2.1~2.3) 완료
    └── WBS0.1_physics_3.1.md          # ✅ Physics Layer (3.1~3.6) 완료
```

<br/>

---

## 🚀 시작하기 (Unity 설정 가이드)

### 1️⃣ 사전 요구사항

| 도구 | 버전 | 링크 |
|:---|:---|:---|
| Unity Hub | 최신 | [다운로드](https://unity.com/download) |
| Unity Editor | **2022.3 LTS** | Unity Hub에서 설치 |
| Android Build Support | Unity와 함께 설치 | Unity Hub Module 추가 |
| AR Foundation | 5.0+ | Package Manager |
| ARCore XR Plugin | 최신 | Package Manager |

### 2️⃣ 프로젝트 열기

```bash
git clone https://github.com/your-repo/MSG.git
# Unity Hub → Add → UnityProject 폴더 선택
```

### 3️⃣ 혈자리 DB 생성

Unity Editor 상단 메뉴:
```
MSG → Parse Acupoints CSV
```
> `Assets/Resources/AcupointDB.asset` 파일이 자동으로 생성됩니다 (361개 혈자리).

### 4️⃣ 씬 구성

**진단 씬 (Scene 0 — `DiagnosisScene`)**
```
Canvas
├── QuestionnaireContainer (Smart)
│   ├── QuestionCardUI   ← CanvasGroup 필수
│   ├── ResultCardUI     ← CanvasGroup 필수
│   ├── LoadingUI
│   ├── ProgressBarUI
│   └── BackButton
└── [빈 GameObject]
    ├── DataFetchService
    ├── DiagnosisStateService
    ├── FallbackRouter
    ├── HandoffService
    └── HandoffData
```

**AR 씬 (Scene 1 — `ARScene`)**
```
AR Session Origin
├── AR Camera
│   └── ARCameraManager
│   └── AROcclusionManager
└── [AR Manager GameObject]
    ├── ARSceneController
    ├── DeepLinkReceiver
    ├── DynamicCunCalibrator
    └── ARActionRenderer
        ├── MarkerPrefab → [연결]
        ├── PathLineRenderer → [연결]
        └── Effect Prefabs (Press/Rub/Tap) → [연결]
```

### 5️⃣ Android 빌드

```
File → Build Settings
├── Platform: Android
├── Minimum API Level: 24 (Android 7.0)
└── Build → 또는 Build And Run
```

`AndroidManifest.xml`에 다음을 추가:
```xml
<uses-permission android:name="android.permission.CAMERA"/>
<uses-feature android:name="android.hardware.camera.ar" android:required="true"/>
```

<br/>

---

## 🧩 핵심 알고리즘

### 동적 촌도(Cun) 캘리브레이션

$$D = \sqrt{(x_2-x_1)^2 + (y_2-y_1)^2 + (z_2-z_1)^2}$$

$$\text{1촌} = \frac{D}{12} \quad \text{(팔꿈치~손목 = 12촌 규칙)}$$

### 1D 칼만 필터 (떨림 제거)

```csharp
// 예측 단계
P += Q
// 칼만 게인
K = P / (P + R)
// 업데이트
X += K * (measurement - X)
P *= (1 - K)
```

### 비선형 스킨 매핑 (곡률 보정)

```
arcLength = cunDistance × OneCunInMeters × curvatureFactor (1.05)
```

<br/>

---

## 📊 WBS 진행 현황

| Phase | 항목 | 상태 |
|:---:|:---|:---:|
| **0.1** | MSG 문진트리 JSON 정규화 | ✅ 완료 |
| **0.2** | 혈자리표 V11 Unity DB화 | ✅ 완료 |
| **1.1** | Unity 환경 셋업 | ✅ 완료 |
| **1.2** | `DataFetchService` 구축 | ✅ 완료 |
| **1.3** | `DiagnosisStateService` 구축 | ✅ 완료 |
| **1.4** | 텍스트 폴백 라우터 | ✅ 완료 |
| **1.5** | Smart / Dumb 컴포넌트 분리 | ✅ 완료 |
| **1.6** | UI/UX 트랜지션 적용 | ✅ 완료 |
| **2.1** | 페이로드 Minification | ✅ 완료 |
| **2.2** | Base64URL 직렬화 로직 | ✅ 완료 |
| **2.3** | AR 가이드 시작 트리거 | ✅ 완료 |
| **3.1** | Unity AR Foundation 셋업 | ✅ 완료 |
| **3.2** | `DeepLinkReceiver` 구축 | ✅ 완료 |
| **3.3** | 동적 촌도 캘리브레이션 | ✅ 완료 |
| **3.4** | 비선형 스킨 매핑 & 안정화 | ✅ 완료 |
| **3.5** | `ARActionRenderer` 시각화 | ✅ 완료 |
| **3.6** | 안착감 및 청각 피드백 | ✅ 완료 |
| **4.1** | Android APK 빌드 최적화 | 🔲 대기 |

<br/>

---

## ⚔️ 아키텍처 수호 원칙 (6대 게이팅 질문)

> ❗ 아래 질문 중 하나라도 **YES**이면 즉시 구현을 중단하고 설계를 롤백합니다.

| # | 게이팅 질문 |
|:---:|:---|
| 1 | 이 기능이 서버 비용이나 외부 API 통신 비용을 발생시키는가? |
| 2 | 확률적 AI(LLM)가 문진 결과 라우팅에 개입할 여지가 있는가? |
| 3 | 페이로드에 Unity 내장 DB에서 이미 참조 가능한 데이터(좌표, 이름)가 중복 포함되었는가? |
| 4 | Dumb 컴포넌트가 다음 라우팅 노드 ID를 직접 계산하는가? |
| 5 | AR 씬이 사용자의 증상(Symptom) 맥락 데이터를 요구하는가? |
| 6 | 옵션 선택 후 다음 화면까지 3초 이상의 딜레이가 발생하는가? |

<br/>

---

## 📖 문서

| 문서 | 설명 |
|:---|:---|
| [`doc/SoT.md`](doc/SoT.md) | 📌 Source of Truth — 아키텍처의 헌법 |
| [`doc/WBS0.1.md`](doc/WBS0.1.md) | 📊 통합 마스터플랜 (WBS) |
| [`doc/first_plan.md`](doc/first_plan.md) | 💡 Unity 단일 앱 전환 결정문서 |
| [`doc/WBS0.1_logic_1.1.md`](doc/WBS0.1_logic_1.1.md) | ✅ Logic Layer 구현 기록 |
| [`doc/WBS0.1_physics_3.1.md`](doc/WBS0.1_physics_3.1.md) | ✅ Physics Layer 구현 기록 |

<br/>

---

<div align="center">

**Project MSG** &nbsp;|&nbsp; *Deterministic. Zero-Cost. Personalized.*

<br/>

*혈자리 데이터: 혈자리표 V11 (361개 경혈)*  
*AR 엔진: Unity 2022.3 LTS + AR Foundation 5.0*

</div>
