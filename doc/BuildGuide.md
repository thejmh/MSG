# 📱 프로젝트 MSG 앱 빌드 가이드 (App Build Guide)

본 가이드는 Angular 21 기반으로 개발된 프로젝트 MSG 웹 애플리케이션을 **Capacitor**를 사용하여 네이티브 모바일 애플리케이션(Android APK / iOS IPA)으로 패키징하는 과정을 단계별로 설명합니다.

---

## 🛠️ 사전 준비 사항 (Prerequisites)
1. **Node.js** (v20 이상 권장, 프로젝트 내 FNM 설정 확인)
2. **Android Studio** (Android 빌드용) 및 **Xcode** (iOS 빌드용, macOS 필수)
3. 모바일 디바이스 (개발자 모드 및 USB 디버깅이 활성화된 기기)

---

## 🚀 단계별 빌드 프로세스

### 1단계: Capacitor 의존성 패키지 설치
프로젝트 루트 경로에서 Capacitor CLI 및 Core 패키지를 설치합니다.
```bash
npm install @capacitor/core @capacitor/cli
```

### 2단계: Capacitor 초기화 설정
앱의 이름, 패키지 ID(Bundle Identifier), 그리고 웹 에셋 빌드 폴더를 구성합니다.
* **패키지 ID**: `com.thejmh.MSG` (기존 안드로이드 매니페스트 및 딥링크 설정과 일치)
* **웹 에셋 경로**: `dist/app/browser` (Angular 21 기본 빌드 아웃풋 경로)
```bash
npx cap init "Meridian Symptom Guide" "com.thejmh.MSG" --web-dir=dist/app/browser
```

### 3단계: 모바일 플랫폼 추가
빌드하려는 타겟 플랫폼에 맞춰 패키지를 설치하고 활성화합니다.

#### Android 플랫폼 추가
```bash
npm install @capacitor/android
npx cap add android
```

#### iOS 플랫폼 추가 (macOS에서만 사용 가능)
```bash
npm install @capacitor/ios
npx cap add ios
```

---

## 🔄 코드 변경 시 동기화 루틴
웹 소스 코드(Angular)를 수정한 뒤 스마트폰 앱에 반영할 때는 항상 아래 루틴을 거쳐야 합니다.

```bash
# 1. Angular 웹 프로젝트 프로덕션 빌드
npx ng build

# 2. 빌드된 산출물을 네이티브 앱 폴더로 동기화
npx cap sync
```

---

## 🏃 기기에서 실행 및 디버깅

### 실제 Android 기기에서 직접 구동하기
USB 디버깅이 켜진 폰을 PC에 연결한 상태에서 아래 명령을 치면 자동으로 Gradle 빌드를 수행하고 앱을 실행합니다.
```bash
npx cap run android
```

### 네이티브 IDE(Android Studio / Xcode) 띄우기
아이콘 변경, 권한 설정, 릴리즈용 서명(Sign) 빌드를 만들기 위해 안드로이드 스튜디오나 Xcode 프로젝트를 직접 열 때 사용합니다.
```bash
# Android Studio 열기
npx cap open android

# Xcode 열기
npx cap open ios
```

---

## 🌐 PWA (Progressive Web App) 배포 (서버리스 최적 대안)
앱스토어 등록 없이 폰의 홈 화면에 추가하여 네이티브 앱처럼 동작시키려면, 웹 서버 호스팅 후 **PWA** 형태로 사용하는 것을 권장합니다.

1. 프로젝트 내에 `@angular/pwa`가 세팅되어 있어 빌드 시 자동으로 `ngsw-config.json`과 서비스 워커가 함께 생성됩니다.
2. 빌드 폴더(`dist/app/browser`)를 GitHub Pages나 Netlify에 무료로 정적 배포합니다.
3. 스마트폰 브라우저에서 배포된 URL로 접속 후, 메뉴에서 **"홈 화면에 추가"** 또는 **"앱 설치"**를 탭합니다.
4. 오프라인 상태에서도 완전한 앱 모드로 무설치 구동됩니다.
