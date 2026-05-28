# WBS 4.1 — 씬 와이어링 & Android 빌드 셋업
**Layer:** Setup | **Task ID:** 4.1 | **작성일:** 2026-05-21

## ✅ 완료된 작업

| 항목 | 결과 |
|------|------|
| `AcupointCunOffset` 테이블 전체 확장 | ✅ 14개 → 131개 (처방 49개 전부 커버) |
| `SceneBuilder.cs` 에디터 스크립트 | ✅ `Assets/Scripts/Editor/SceneBuilder.cs` |
| `PrefabBuilder.cs` 에디터 스크립트 | ✅ `Assets/Scripts/Editor/PrefabBuilder.cs` |
| `AndroidManifest.xml` | ✅ `Assets/Plugins/Android/AndroidManifest.xml` |

---

## 📋 Unity Editor에서 실행할 순서

### Step 1 — AR Foundation 패키지 설치
Unity 메뉴 → **Window > Package Manager**
- `AR Foundation` 5.0+ 설치
- `ARCore XR Plugin` 설치

### Step 2 — AcupointDB 생성
Unity 메뉴 → **MSG > Parse Acupoints CSV**
- `Assets/Resources/AcupointDB.asset` 자동 생성
- 361개 혈자리 ScriptableObject 완성

### Step 3 — 전체 셋업 실행
Unity 메뉴 → **MSG > [SETUP] Full Project Setup (Run This First)**
- OptionButton.prefab 자동 생성
- DiagnosisScene 자동 빌드 (모든 컴포넌트 와이어링 포함)
- ARScene 자동 빌드
- Build Settings에 두 씬 자동 등록

### Step 4 — Android 빌드
1. **File > Build Settings** → Android 플랫폼 선택 → Switch Platform
2. **Player Settings** 확인:
   - Package Name: `com.msg.meridian`
   - Minimum API Level: Android 7.0 (API 24) 이상
   - Target API Level: Android 14 (API 34) 권장
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64 체크
3. **Build And Run** → `.apk` 생성

---

## 🔑 생성된 파일 목록

| 파일 | 역할 |
|------|------|
| `Assets/Scripts/Editor/SceneBuilder.cs` | DiagnosisScene + ARScene 자동 구성 |
| `Assets/Scripts/Editor/PrefabBuilder.cs` | OptionButton.prefab 자동 생성 |
| `Assets/Plugins/Android/AndroidManifest.xml` | 카메라 권한 + msg-app:// 딥링크 |

---

## ⚠️ 주의사항

- `SceneBuilder`가 생성하는 씬은 `Assets/Scenes/` 폴더에 저장됨.
  기존 `Assets/Scenes/DiagnosisScene.unity`, `ARScene.unity`와 **경로가 다를 수 있음**.
  기존 씬 파일이 있다면 덮어쓰기 전 백업 권장.
- AR Foundation 패키지 미설치 상태에서 `ARSceneController.cs`의 `ARSession`, `ARCameraManager` 등 참조가 컴파일 에러를 낼 수 있음.
  패키지 설치 후 씬 빌드 실행할 것.
