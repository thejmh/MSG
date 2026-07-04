<!-- ╔═══════════════════════════════════════════════════════════════╗ -->
<!-- ║                      MERIDIAN SYMPTOM GUIDE                    ║ -->
<!-- ╚═══════════════════════════════════════════════════════════════╝ -->

<div align="center">

<a href="https://github.com/thejmh/MSG">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=0:071A12,40:0B3D2E,100:34D399&height=230&section=header&text=MSG&fontSize=92&fontColor=F5FBF8&fontAlignY=36&desc=Meridian%20Symptom%20Guide&descSize=22&descAlignY=60&descColor=8FD9B6" width="100%" alt="MSG — Meridian Symptom Guide" />
</a>

<br/>

<a href="https://readme-typing-svg.demolab.com">
  <img src="https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=600&size=22&pause=1200&color=34D399&center=true&vCenter=true&width=720&height=44&lines=AI%EA%B0%80+%EC%95%84%EB%8B%88%EB%9D%BC%2C+%EC%88%98%ED%95%99%EC%9C%BC%EB%A1%9C+%EC%A7%84%EB%8B%A8%ED%95%A9%EB%8B%88%EB%8B%A4.;%EA%B2%B0%EC%A0%95%EB%A1%A0%EC%A0%81+%EB%85%BC%EB%A6%AC%EB%A7%9D+%C2%B7+361%ED%98%88+%EC%A7%80%EC%95%95+%EA%B0%80%EC%9D%B4%EB%93%9C;Zero+Hallucination+%C2%B7+Zero+Server+%C2%B7+Zero+Cost" alt="typing tagline" />
</a>

<br/><br/>

<!-- ◍ meridian badge row ◍ -->
<img src="https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular&logoColor=white&labelColor=0B1F17" alt="Angular 21" />
<img src="https://img.shields.io/badge/TypeScript-5.x-3178C6?style=for-the-badge&logo=typescript&logoColor=white&labelColor=0B1F17" alt="TypeScript" />
<img src="https://img.shields.io/badge/TailwindCSS-v4-38BDF8?style=for-the-badge&logo=tailwindcss&logoColor=white&labelColor=0B1F17" alt="TailwindCSS v4" />
<img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white&labelColor=0B1F17" alt="Docker" />
<img src="https://img.shields.io/badge/PWA-Offline-34D399?style=for-the-badge&logo=pwa&logoColor=0B1F17&labelColor=0B1F17" alt="PWA Offline" />

<br/>

<img src="https://img.shields.io/badge/진단_방식-결정론적_DAG-E4572E?style=flat-square&labelColor=0B1F17" alt="Deterministic DAG" />
<img src="https://img.shields.io/badge/경혈점-361_points-E9C46A?style=flat-square&labelColor=0B1F17" alt="361 acupoints" />
<img src="https://img.shields.io/badge/서버_비용-₩0-34D399?style=flat-square&labelColor=0B1F17" alt="Zero cost" />
<img src="https://img.shields.io/badge/모바일-Capacitor-119EDA?style=flat-square&labelColor=0B1F17" alt="Capacitor" />

</div>

<br/>

> **경락(經絡)의 지혜를, 알고리즘의 확실성으로.**
> MSG는 증상을 **결정론적 논리망**으로 진단하고, 고해상도 인체 해부도 위에
> 정확한 **지압점(경혈)** 위치를 짚어주는 완전 클라이언트-사이드 웹 애플리케이션입니다.
> LLM에 의존하지 않으므로 **같은 증상엔 언제나 같은 결과** — 환각도, 서버도, 비용도 없습니다.

<br/>

---

## ◉ 왜 MSG인가

> [!IMPORTANT]
> 건강 정보를 다루는 도구에서 **재현성**은 타협 대상이 아닙니다.
> 생성형 AI는 같은 질문에도 매번 다른 답을 내놓고, 존재하지 않는 혈자리를 지어낼 수 있습니다.
> MSG는 그 리스크를 **아키텍처 차원에서 제거**합니다.

<div align="center">

|  | 🤖 생성형 AI 진단 | 🧭 **MSG 결정론적 진단** |
|:--|:--:|:--:|
| **재현성** | 매번 다른 답변 | ✅ 동일 입력 → 동일 결과 |
| **환각 위험** | 없는 혈자리 생성 가능 | ✅ 정의된 데이터만 참조 |
| **동작 환경** | 서버·API 키 필요 | ✅ 100% 브라우저 로컬 |
| **오프라인** | 불가 | ✅ PWA 완전 지원 |
| **운영 비용** | 토큰당 과금 | ✅ **₩0** |
| **감사 추적** | 블랙박스 | ✅ 논리 경로 100% 투명 |

</div>

<br/>

## ◉ 시그니처 — 진단 논리망 (Deterministic DAG)

MSG의 심장은 **10개 노드의 방향성 비순환 그래프(DAG)** 입니다.
사용자의 응답은 트리를 따라 **하나의 확정된 경로**로 흐르며, 26개 증상 진단과
그에 매핑된 경혈 처방으로 귀결됩니다. 분기(分岐)마다 결과가 결정되어 있어
같은 길을 걸으면 같은 곳에 도착합니다.

```mermaid
%%{init: {'theme':'base','themeVariables':{'primaryColor':'#0B3D2E','primaryTextColor':'#F5FBF8','primaryBorderColor':'#34D399','lineColor':'#8FD9B6','fontfamily':'JetBrains Mono','clusterBkg':'#071A12','clusterBorder':'#2E8B57'}}}%%
flowchart TD
    START([🩺 증상 입력]) --> Q1{통증 부위?}

    Q1 -->|머리·목| H{두통 양상}
    Q1 -->|어깨·등| B{긴장 위치}
    Q1 -->|팔·다리| L{관절 vs 근육}

    H -->|욱신거림| DX1[진단 · 긴장성 두통]
    H -->|한쪽 편중| DX2[진단 · 편두통 패턴]
    B -->|승모근| DX3[진단 · 어깨 결림]
    L -->|관절| DX4[진단 · 관절 뻣뻣함]

    DX1 --> RX1[[처방 · 태양 · 풍지]]
    DX2 --> RX2[[처방 · 합곡 · 사죽공]]
    DX3 --> RX3[[처방 · 견정 · 천종]]
    DX4 --> RX4[[처방 · 곡지 · 족삼리]]

    RX1 --> MAP((🗺️ 해부도 맵핑))
    RX2 --> MAP
    RX3 --> MAP
    RX4 --> MAP

    classDef q fill:#0B3D2E,stroke:#34D399,color:#F5FBF8;
    classDef dx fill:#12261C,stroke:#E9C46A,color:#F5FBF8;
    classDef rx fill:#2A1410,stroke:#E4572E,color:#FFE7DF;
    classDef map fill:#071A12,stroke:#34D399,color:#34D399,stroke-width:3px;
    class Q1,H,B,L q;
    class DX1,DX2,DX3,DX4 dx;
    class RX1,RX2,RX3,RX4 rx;
    class MAP map;
```

> [!NOTE]
> 위 그래프는 실제 `logic-tree.json` 구조를 단순화해 시각화한 예시입니다.
> 전체 트리는 **10 노드 → 26 증상 → 361 경혈 좌표**로 확장됩니다.

<br/>

## ◉ 핵심 기능

<table>
<tr>
<td width="50%" valign="top">

### 🧭 결정론적 진단
LLM 없이 순수 수학적 의사결정 트리(DAG)로 동작.
**환각 위험 0%**, 모든 진단 경로가 추적 가능합니다.

</td>
<td width="50%" valign="top">

### 🫀 인터랙티브 해부도
머리·가슴·등·팔·다리 **5개 부위**의 고해상도 일러스트.
다크모드에 최적화되어 경혈점이 선명하게 대비됩니다.

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📴 완전 오프라인
Service Worker 기반 PWA. 한 번 로드하면
**네트워크 없이도** 전 기능이 동작합니다.

</td>
<td width="50%" valign="top">

### 💸 인프라 비용 ₩0
서버·DB·API 키가 필요 없습니다.
모든 연산이 **브라우저 로컬**에서 끝납니다.

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📱 네이티브 패키징
**Capacitor** 연동으로 iOS·Android 앱으로
그대로 빌드할 수 있습니다.

</td>
<td width="50%" valign="top">

### 🐳 Docker 원클릭
복잡한 로컬 세팅 없이 컨테이너 하나로
어디서든 동일하게 배포됩니다.

</td>
</tr>
</table>

<br/>

## ◉ 기술 스택

<div align="center">

| 레이어 | 기술 | 역할 |
|:--:|:--|:--|
| **Framework** | `Angular 21` | 단일 SPA 애플리케이션 셸 |
| **Language** | `TypeScript 5.x` | 타입 안전 로직 계층 |
| **Styling** | `TailwindCSS v4` | 유틸리티 우선 다크 UI |
| **Delivery** | `PWA · Service Worker` | 오프라인 캐싱 & 설치형 앱 |
| **Native** | `Capacitor` | iOS / Android 네이티브 래핑 |
| **Deploy** | `Docker` | 컨테이너 배포 |

</div>

<br/>

## ◉ 데이터 아키텍처

MSG는 **데이터 주도(data-driven)** 설계입니다. 로직과 콘텐츠가 JSON으로 분리되어,
코드를 건드리지 않고도 진단 규칙과 경혈 데이터를 확장할 수 있습니다.

```text
public/
├── ◍ logic-tree.json      # 10-노드 진단 경로 그래프 (DAG)
├── ◍ diagnostics.json     # 26개 증상 → 경혈 처방 매핑 테이블
├── ◍ acupoints.json       # 361개 경혈 좌표 데이터
└── ◍ *_anatomy.png        # 부위별 해부도 (head · chest · back · arms · legs)

src/app/
├── components/            # 진단 · 해부도 맵핑 · 경혈 상세 UI
└── services/              # 데이터 페칭 로직

Dockerfile                # 컨테이너 정의
doc/                      # 설계 문서 & 빌드 가이드
```

<br/>

## ◉ 빠른 시작

### 🐳 Docker (권장)

```bash
# 1. 이미지 빌드
docker build -t msg-angular-app .

# 2. 컨테이너 실행
docker run -d -p 3000:3000 --name msg-app msg-angular-app

# 3. 브라우저에서 열기
#    → http://localhost:3000
```

### 📱 모바일 빌드 (Capacitor)

네이티브 iOS / Android 패키징은 Capacitor로 지원됩니다.
자세한 절차는 [`doc/BuildGuide.md`](doc/BuildGuide.md)를 참고하세요.

<br/>

---

<div align="center">

### 경혈은 몸이 그려둔 지도. MSG는 그 지도를 읽어줄 뿐입니다. ◉

<sub>본 프로젝트는 정보 제공용 도구이며, 전문 의료 진단을 대체하지 않습니다.</sub>

<br/>

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:34D399,60:0B3D2E,100:071A12&height=120&section=footer" width="100%" alt="footer" />

</div>
