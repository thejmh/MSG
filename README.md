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

*당신의 고통을 결정론적 논리로 진단하고, 인터랙티브 해부도 위에서 최적의 지압 가이드를 표출합니다*

<br/>

[![Angular](https://img.shields.io/badge/Angular-21.0-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.dev)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-v4.0-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)](https://tailwindcss.com)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com)
[![Zero Cost](https://img.shields.io/badge/Infra_Cost-$0-FFE66D?style=for-the-badge&logo=serverless&logoColor=black)](https://github.com)

<br/>

</div>

---

<br/>

## 🧠 프로젝트 소개

> **"사용자의 모호한 신체적 고통을 결정론적 논리망으로 진단하고,  
> 도출된 혈자리 솔루션을 고화질 2D 인체 해부학적 지도 위에 매핑하여  
> 개인화된 지압 위치와 방법을 시각화하는 초개인화 무과금 건강 가이드 시스템"**

프로젝트 MSG는 **확률론적 AI(LLM)의 환각(Hallucination)을 원천 차단**하고,  
순수한 **수학적 결정 트리(DAG)**와 표준 침구학 데이터를 기반으로 경락/혈자리 마사지를 안내합니다.  
서버 인프라 비용은 완전히 **$0**입니다.

<br/>

---

## ✨ 핵심 특징

<div align="center">

|  | 특징 | 설명 |
|:---:|:---|:---|
| 🧬 | **결정론적 진단** | LLM 없음. 수학적 DAG 트리만으로 100% 신뢰도 높은 결과 도출 |
| 🌐 | **단일 플랫폼 통합** | 웹(Angular 21) 하나로 진단부터 인터랙티브 해부도 가이드까지 완성 |
| 🎨 | **고해상도 해부도** | 다크 모드에 맞춰 특화된 정밀 네온 해부도 일러스트 5종 기본 장착 |
| 💰 | **Zero-Cost 인프라** | 서버/DB 비용 제로. 완전한 클라이언트 단독 구동 및 로컬 캐싱 |
| 📴 | **오프라인 구동 (PWA)** | 서비스 워커 탑재로 네트워크 없이 기기 단독 100% 작동 |
| 📦 | **도커(Docker) 구동** | 복잡한 로컬 패키지 설치 없이 컨테이너화된 테스트 및 구동 지원 |

</div>

<br/>

---

## 🏗️ 아키텍처 및 폴더 구조

```
c:\Users\isaac\Downloads\MSG
├── public/                 # 정적 자산 (진단 트리 JSON, 처방 JSON, 해부도 PNG)
│   ├── logic-tree.json     # 10개 문진 노드 및 26개 결과 매핑 트리
│   ├── diagnostics.json    # 26개 증상별 혈자리 처방 테이블
│   ├── acupoints.json      # 361개 혈자리 위치 설명, X/Y% 좌표 DB
│   └── *_anatomy.png       # 부위별(머리, 가슴, 등, 팔, 다리) 해부학 이미지
├── src/
│   └── app/
│       ├── components/     # 문진(diagnosis), 지도(anatomy-map), 지압(acupoint-detail)
│       └── services/       # 데이터 페치(data.service)
├── Dockerfile              # 로컬 구동용 도커 컨테이너 정의 파일
└── doc/                    # 마스터 설계(SoT.md) 및 모바일 앱 빌드 가이드(BuildGuide.md)
```

<br/>

---

## 🚀 시작하기 (Getting Started)

### Docker로 실행하기 (추천)
로컬에 별도의 개발 도구(Node.js 등)를 설치할 필요 없이 도커만으로 즉시 안전하게 실행할 수 있습니다.

```bash
# 1. 도커 이미지 빌드
docker build -t msg-angular-app .

# 2. 도커 컨테이너 실행 (포트 3000 바인딩)
docker run -d -p 3000:3000 --name msg-app msg-angular-app
```
실행이 완료되면 브라우저에서 **`http://localhost:3000`**에 접속하여 앱을 확인할 수 있습니다.

<br/>

---

## 📱 모바일 앱 빌드 및 배포
본 프로젝트는 **Capacitor**를 통해 즉시 네이티브 모바일 애플리케이션으로 패키징될 수 있도록 사양 설계가 적용되어 있습니다. 자세한 빌드 커맨드와 단계별 프로세스는 [BuildGuide.md](file:///c:/Users/isaac/Downloads/MSG/doc/BuildGuide.md) 문서를 참고해 주시기 바랍니다.
