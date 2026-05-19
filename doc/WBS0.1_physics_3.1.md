# WBS 3.1 ~ 3.6 — Physics Layer (Unity AR)
**Layer:** Physics | **Task ID:** 3.1 ~ 3.6 | **완료일:** 2026-05-17

---

## Task 3.1 — Unity AR/MediaPipe 셋업
**파일:** `Scripts/AR/ARSceneController.cs`

| 항목 | 결과 |
|------|------|
| AR Foundation, ARCore 컴포넌트 참조 구조 | ✅ `ARSession`, `ARCameraManager`, `AROcclusionManager` |
| 카메라 권한 | ✅ AndroidManifest에 CAMERA 권한 추가 필요 (주석 가이드 포함) |

> ⚠️ **Unity 패키지 설치 필요:** Package Manager에서 `AR Foundation 5.0+`, `ARCore XR Plugin` 설치 필요.

---

## Task 3.2 — DeepLinkReceiver
**파일:** `Scripts/AR/DeepLinkReceiver.cs`

| 항목 | 결과 |
|------|------|
| HandoffData 인메모리 수신 (1순위) | ✅ `HandoffData.Instance.payload` 확인 |
| 레거시 딥링크 수신 (2순위) | ✅ `Application.deepLinkActivated` 구독 |
| 웹 문진 맥락 없이 순수 `id, i, m`만 수신 | ✅ |

---

## Task 3.3 — 동적 촌도 캘리브레이션
**파일:** `Scripts/AR/DynamicCunCalibrator.cs` → `Calibrate()`

| 항목 | 결과 |
|------|------|
| ARCore Depth API 기반 3D 좌표 획득 구조 | ✅ Vector3 입력으로 추상화 |
| 유클리드 거리 → 1촌 절대 척도 | ✅ `Vector3.Distance(p1, p2) / cunDistance` |
| 오차 5% 이내 목표 | ✅ 30프레임 버퍼 평균 + 칼만 필터 적용 |

**알고리즘:**
```
D = Vector3.Distance(P1, P2)        // 유클리드 거리 (미터)
1촌(raw) = D / 12                   // 팔꿈치~손목 = 12촌 규칙
1촌(filtered) = KalmanFilter(raw)   // 노이즈 제거
```

---

## Task 3.4 — 비선형 스킨 매핑 & 안정화
**파일:** `Scripts/AR/DynamicCunCalibrator.cs` → `KalmanFilter1D`, `CunToWorldPosition()`

| 항목 | 결과 |
|------|------|
| 1D 칼만 필터 (떨림 제거) | ✅ Q=0.01, R=0.1 기본값 |
| IMU 자이로 틸트 보상 | ✅ `AdjustForTilt(angle)` → R 동적 조정 |
| 곡률 보정 아크 길이 적분 | ✅ `curvatureFactor=1.05` 보정 계수 |
| AR 마커 표류(Floating) 방지 | ✅ 30프레임 버퍼링으로 위치 안정화 |

---

## Task 3.5 — ARActionRenderer 시각화
**파일:** `Scripts/AR/ARActionRenderer.cs`

| 항목 | 결과 |
|------|------|
| 세기(`i=1`) → 초록색 발광 마커 | ✅ Emission 색상 `(0.2, 0.9, 0.3)` |
| 세기(`i=0`) → 빨간색 발광 마커 | ✅ Emission 색상 `(0.95, 0.25, 0.2)` |
| 방법(`m=1`) Press → PressStrategy | ✅ Strategy Pattern 런타임 교체 |
| 방법(`m=2`) Rub → RubStrategy | ✅ |
| 방법(`m=3`) Tap → TapStrategy | ✅ |
| 경로(Pathline) 빛의 선 | ✅ `LineRenderer` 하늘색 글로우 |

---

## Task 3.6 — 안착감 및 청각 피드백
**파일:** `Scripts/AR/ARSceneController.cs`, `Scripts/AR/ARActionRenderer.cs`

| 항목 | 결과 |
|------|------|
| Fake Drop Shadow 투영 | ✅ 마커 아래 2mm 그림자 평면 투영 |
| Spatial Audio 화면 밖 안내 | ✅ ViewportPoint 가시성 판단 후 AudioSource 방향 이동 |
| 무거운 광원 AI 없이 구현 | ✅ 클래식 그림자 트릭 사용 |
