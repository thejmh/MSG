using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using MSG.Services;
using MSG.Models;

namespace MSG.AR
{
    /// <summary>
    /// [WBS 3.1 ~ 3.6] ARSceneController — 터치 기반 신체 앵커링 AR 시스템
    ///
    /// 사용자 플로우:
    ///   1. AR 씬 진입 → 실제 카메라 피드 활성화
    ///   2. 안내: "손목 부위를 탭하세요" → 탭 → ARRaycast로 3D 좌표 획득
    ///   3. 안내: "팔꿈치 부위를 탭하세요" → 탭 → 3D 좌표 획득
    ///   4. DynamicCunCalibrator.Calibrate(손목, 팔꿈치, 12촌) → 1촌 계산
    ///   5. 혈자리별 촌도 오프셋으로 실제 3D 위치 계산
    ///   6. 각 위치에 AcupointAnchor 마커 배치 (실제 팔 위에 고정)
    ///
    /// 에디터 시뮬레이션:
    ///   - ARRaycast 불가 → 마우스 클릭 위치를 카메라 앞 0.5m 깊이 3D 좌표로 변환
    ///   - 기능 동작 검증 가능
    /// </summary>
    public class ARSceneController : MonoBehaviour
    {
        // ── 캘리브레이션 상태 머신 ────────────────────────────────────
        private enum CalibrationState
        {
            Idle,               // 초기 대기
            WaitingForWrist,    // 손목 탭 대기
            WaitingForElbow,    // 팔꿈치 탭 대기
            Calibrated,         // 캘리브레이션 완료
            Rendering           // 마커 렌더링 완료
        }

        [Header("AR Foundation Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private ARRaycastManager arRaycastManager;

        [Header("하위 컴포넌트")]
        [SerializeField] private DynamicCunCalibrator cunCalibrator;
        [SerializeField] private ARActionRenderer actionRenderer;
        [SerializeField] private DeepLinkReceiver deepLinkReceiver;

        [Header("Spatial Audio")]
        [SerializeField] private AudioSource spatialAudioSource;
        [SerializeField] private AudioClip guideBeepClip;

        // ── 내부 상태 ────────────────────────────────────────────────
        private CalibrationState _state = CalibrationState.Idle;
        private Vector3 _wristWorldPos;
        private Vector3 _elbowWorldPos;
        private HandoffPayload _payload;
        private readonly List<AcupointAnchor> _spawnedAnchors = new List<AcupointAnchor>();
        private ARHUDController _hudController;

        // AR Raycast 결과 재사용 (GC 최소화)
        private static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        // 에디터 시뮬레이션 깊이
        private const float EDITOR_DEPTH = 0.5f;

        // ── 탭 피드백용 시각 마커 ────────────────────────────────────
        private GameObject _wristTapMarker;
        private GameObject _elbowTapMarker;

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            // 컴포넌트 자동 탐색 (인스펙터 연결 누락 시 복원)
            if (cunCalibrator == null)   cunCalibrator   = FindFirstObjectByType<DynamicCunCalibrator>();
            if (actionRenderer == null)  actionRenderer  = FindFirstObjectByType<ARActionRenderer>();
            if (deepLinkReceiver == null) deepLinkReceiver = FindFirstObjectByType<DeepLinkReceiver>();
            if (arRaycastManager == null) arRaycastManager = FindFirstObjectByType<ARRaycastManager>();

            // ARHUDController 자동 스폰
            _hudController = FindFirstObjectByType<ARHUDController>();
            if (_hudController == null)
            {
                var hudGo = new GameObject("AR_HUD_AutoController");
                _hudController = hudGo.AddComponent<ARHUDController>();
                Debug.Log("[ARSceneController] ARHUDController 자동 스폰.");
            }

            // ── 에디터 전용: ARCameraBackground 검은 화면 우회 ──────────
            // 에디터에는 물리적 AR 카메라가 없으므로 ARCameraBackground가
            // 검은 화면을 출력합니다. 솔리드 다크 배경으로 대체합니다.
            // 실제 기기 빌드 시에는 이 블록이 실행되지 않습니다.
            if (Application.isEditor)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.08f, 0.10f, 0.15f, 1f);

                    var arBg = cam.GetComponent<ARCameraBackground>();
                    if (arBg != null) arBg.enabled = false;
                }
                Debug.Log("[ARSceneController] 에디터 모드: ARCameraBackground 비활성화, 다크 배경 적용.");
            }
        }


        private void Start()
        {
            // AR 세션 활성화
            if (arSession != null)
                arSession.enabled = true;

            // ── DataFetchService 자동 보장 ────────────────────────────
            // ARScene에 DataFetchService가 없으면 자동 생성 (DontDestroyOnLoad로 넘어오지 않은 경우 대비)
            if (DataFetchService.Instance == null)
            {
                var svcGo = new GameObject("DataFetchService_AutoSpawned");
                svcGo.AddComponent<DataFetchService>();
                Debug.LogWarning("[ARSceneController] DataFetchService 자동 생성. " +
                                 "DiagnosisScene에서 넘어온 경우 정상. 직접 ARScene 실행 시 데이터 로드 대기 필요.");
            }

            // 에디터 전용: 가상 페이로드 주입 (단독 재생 테스트용)
            if (Application.isEditor && (HandoffData.Instance == null || HandoffData.Instance.payload == null))
            {
                HandoffData.Instance.payload = new HandoffPayload
                {
                    dId = "res_lower_back",
                    pts = new List<MeridianPoint>
                    {
                        new MeridianPoint { id = 148, i = 1, m = 1 }, // 신유
                        new MeridianPoint { id = 165, i = 1, m = 1 }, // 위중
                        new MeridianPoint { id = 337, i = 0, m = 2 }  // 명문
                    }
                };
                Debug.Log("[ARSceneController] 에디터 가상 페이로드 주입 완료.");
            }

            // 페이로드 로드
            if (HandoffData.Instance?.payload != null)
            {
                _payload = HandoffData.Instance.payload;
                // DataFetchService 로드 완료 후 캘리브레이션 시작
                if (DataFetchService.Instance != null && DataFetchService.Instance.IsLoaded)
                    BeginCalibration();
                else if (DataFetchService.Instance != null)
                    DataFetchService.Instance.OnDataLoaded += OnDataServiceLoaded;
                else
                    BeginCalibration(); // 최후 폴백
            }
            else if (deepLinkReceiver != null)
            {
                deepLinkReceiver.OnPayloadReceived += OnPayloadReceived;
            }
        }

        private void OnDataServiceLoaded()
        {
            if (DataFetchService.Instance != null)
                DataFetchService.Instance.OnDataLoaded -= OnDataServiceLoaded;
            BeginCalibration();
        }

        private void OnPayloadReceived(HandoffPayload payload)
        {
            _payload = payload;
            BeginCalibration();
        }

        // ── 캘리브레이션 시작 ─────────────────────────────────────────
        public void BeginCalibration()
        {
            cunCalibrator?.Reset();
            ClearAnchors();

            // 기존에 남아있던 탭 피드백 마커 제거
            if (_wristTapMarker != null) Destroy(_wristTapMarker);
            if (_elbowTapMarker != null) Destroy(_elbowTapMarker);

            _state = CalibrationState.WaitingForWrist;

            _hudController?.SetCalibrationState(
                CalibrationPhase.WaitingForWrist,
                "📍 손목 안쪽(요골 동맥 부위)을 화면에서 탭하세요"
            );
            Debug.Log("[ARSceneController] 캘리브레이션 시작 → 손목 탭 대기");
        }

        // ── 입력 처리 (Input System Package) ────────────────────────
        private void Update()
        {
            // 가이드 안내 팝업이 떠 있는 동안에는 입력을 처리하지 않음
            if (_hudController != null && _hudController.IsInstructionPopupActive)
                return;

            if (_state != CalibrationState.WaitingForWrist &&
                _state != CalibrationState.WaitingForElbow)
                return;

            // 터치 입력 (Input System 터치스크린)
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var primaryTouch = touchscreen.primaryTouch;
                if (primaryTouch.press.wasPressedThisFrame)
                {
                    Vector2 pos = primaryTouch.position.ReadValue();
                    HandleTap(pos);
                }
                return;
            }

            // 마우스 클릭 (에디터 시뮬레이션)
            var mouse = Mouse.current;
            if (Application.isEditor && mouse != null && mouse.leftButton.wasPressedThisFrame)
                HandleTap(mouse.position.ReadValue());
        }

        private void HandleTap(Vector2 screenPos)
        {
            bool hitFound = TryGetWorldPosition(screenPos, out Vector3 worldPos);
            if (!hitFound) return;

            switch (_state)
            {
                case CalibrationState.WaitingForWrist:
                    OnWristTapped(worldPos);
                    break;
                case CalibrationState.WaitingForElbow:
                    OnElbowTapped(worldPos);
                    break;
            }
        }

        /// <summary>
        /// 화면 좌표 → 3D 세계 좌표 변환.
        /// 실제 기기: ARRaycastManager로 AR 평면/특징점 레이캐스트.
        /// 에디터: 카메라 앞 고정 깊이로 시뮬레이션.
        /// </summary>
        private bool TryGetWorldPosition(Vector2 screenPos, out Vector3 worldPos)
        {
            // 실제 기기: AR Foundation 레이캐스트
            if (!Application.isEditor && arRaycastManager != null)
            {
                if (arRaycastManager.Raycast(screenPos, RaycastHits,
                    TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint))
                {
                    worldPos = RaycastHits[0].pose.position;
                    return true;
                }
                // AR 평면이 아직 감지 안된 경우 FeaturePoint만으로 재시도
                if (arRaycastManager.Raycast(screenPos, RaycastHits, TrackableType.FeaturePoint))
                {
                    worldPos = RaycastHits[0].pose.position;
                    return true;
                }
            }

            // 에디터 시뮬레이션: 화면 좌표 → 카메라 앞 0.5m
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, EDITOR_DEPTH);
                worldPos = cam.ScreenToWorldPoint(screenPoint);
                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }

        // ── 손목 탭 처리 ─────────────────────────────────────────────
        private void OnWristTapped(Vector3 worldPos)
        {
            _wristWorldPos = worldPos;
            _state = CalibrationState.WaitingForElbow;

            // 탭 위치 시각 마커 표시
            _wristTapMarker = CreateTapFeedbackMarker(worldPos, new Color(0.3f, 0.8f, 1f), "손목");

            _hudController?.SetCalibrationState(
                CalibrationPhase.WaitingForElbow,
                "✅ 손목 위치 확인!\n📍 이제 팔꿈치 안쪽을 탭하세요"
            );
            Debug.Log($"[ARSceneController] 손목 위치 확정: {worldPos}");
        }

        // ── 팔꿈치 탭 처리 ───────────────────────────────────────────
        private void OnElbowTapped(Vector3 worldPos)
        {
            _elbowWorldPos = worldPos;
            _state = CalibrationState.Calibrated;

            // 탭 위치 시각 마커 표시
            _elbowTapMarker = CreateTapFeedbackMarker(worldPos, new Color(1f, 0.5f, 0.2f), "팔꿈치");

            // 두 점이 너무 가까우면 에러
            float dist = Vector3.Distance(_wristWorldPos, _elbowWorldPos);
            if (dist < 0.05f)
            {
                _hudController?.SetCalibrationState(
                    CalibrationPhase.Error,
                    "⚠️ 두 점이 너무 가깝습니다.\n다시 처음부터 시도하세요."
                );
                StartCoroutine(RetryCalibrationAfterDelay(2.5f));
                return;
            }

            // 캘리브레이션 실행
            cunCalibrator.Calibrate(_wristWorldPos, _elbowWorldPos, 12f);

            float oneCunCm = cunCalibrator.OneCunInMeters * 100f;
            _hudController?.SetCalibrationState(
                CalibrationPhase.Calibrated,
                $"✅ 캘리브레이션 완료!\n1촌 = {oneCunCm:F1}cm\n아래의 혈자리 처방을 확인하세요."
            );

            Debug.Log($"[ARSceneController] 팔꿈치 위치 확정: {worldPos} | 팔 길이: {dist * 100f:F1}cm");

            // 탭 피드백 마커 즉시 소멸 처리
            if (_wristTapMarker != null) Destroy(_wristTapMarker);
            if (_elbowTapMarker != null) Destroy(_elbowTapMarker);

            // 혈자리 정보 카드 표시
            _hudController?.ShowAcupointList(_payload);
        }

        // ── 3D 혈자리 마커 계산 및 표시 ───────────────────────────────────────
        public void RenderAcupointMarkers()
        {
            if (_payload == null || cunCalibrator == null || !cunCalibrator.IsCalibrated) return;

            ClearAnchors();

            // 탭 피드백 마커 제거
            if (_wristTapMarker != null) Destroy(_wristTapMarker);
            if (_elbowTapMarker != null) Destroy(_elbowTapMarker);

            var worldPositions = new List<Vector3>();
            int idx = 1;

            foreach (var pt in _payload.pts)
            {
                // 혈자리 메타데이터 조회
                AcupointEntry acupointData = null;
                if (DataFetchService.Instance != null)
                    acupointData = DataFetchService.Instance.GetAcupoint(pt.id);

                // 촌도 기반 3D 좌표 계산
                Vector3 markerPos = cunCalibrator.GetAcupointWorldPosition(pt.id);
                worldPositions.Add(markerPos);

                // AcupointAnchor 생성
                var anchorGo = new GameObject($"AcupointAnchor_{pt.id}_{acupointData?.pointName ?? "unknown"}");
                anchorGo.transform.position = markerPos;

                var anchor = anchorGo.AddComponent<AcupointAnchor>();
                anchor.Initialize(pt.id, pt.i, pt.m, acupointData, idx);
                _spawnedAnchors.Add(anchor);

                string ptName = acupointData?.pointName ?? $"ID:{pt.id}";
                Debug.Log($"[ARSceneController] 혈자리 마커 배치: {ptName} @ {markerPos} (오프셋: {cunCalibrator.GetCunOffset(pt.id):F1}촌)");
                idx++;
            }

            // 경로선 렌더링 (기존 ARActionRenderer 재활용)
            if (actionRenderer != null)
            {
                var receivedPoints = new List<ReceivedPoint>();
                foreach (var pt in _payload.pts)
                    receivedPoints.Add(new ReceivedPoint { id = pt.id, i = pt.i, m = pt.m });

                // 경로선만 렌더링 (마커는 AcupointAnchor가 처리)
                actionRenderer.RenderPathlineOnly(worldPositions);
            }

            _state = CalibrationState.Rendering;
            _hudController?.SetCalibrationState(
                CalibrationPhase.Done,
                "✨ 혈자리 마커 표시 완료!\n각 혈자리를 확인하고 지압하세요."
            );
            _hudController?.ShowAcupointList(_payload);

            // Spatial Audio 가이드
            StartCoroutine(PlaySpatialAudioGuide(worldPositions));
        }

        // ── 재시도 ───────────────────────────────────────────────────
        private IEnumerator RetryCalibrationAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            BeginCalibration();
        }

        // ── 탭 피드백 마커 ───────────────────────────────────────────
        private GameObject CreateTapFeedbackMarker(Vector3 worldPos, Color color, string label)
        {
            // ARMeshHelper를 사용하여 물리 셰이프 에러 방지
            var go = ARMeshHelper.CreateVisualMarker($"TapMarker_{label}", color, 0.015f, worldPos, true);
            return go;
        }

        // ── 수동 캘리브레이션 (HUD 버튼용 폴백) ─────────────────────
        /// <summary>에디터/기기에서 HUD 버튼으로 즉시 강제 캘리브레이션.</summary>
        public void TriggerManualCalibration()
        {
            if (cunCalibrator == null) return;

            // 화면 중앙 기준으로 0.4m~0.7m 앞에 가상 팔 좌표 생성
            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : Vector3.forward;
            Vector3 camPos = cam != null ? cam.transform.position : Vector3.zero;
            Vector3 camRight = cam != null ? cam.transform.right : Vector3.right;

            // 화면 가운데 아래쪽 = 손목, 위쪽 = 팔꿈치 (세로 화면 기준)
            _wristWorldPos = camPos + camForward * 0.45f - cam.transform.up * 0.12f;
            _elbowWorldPos = camPos + camForward * 0.45f + cam.transform.up * 0.264f;

            cunCalibrator.Calibrate(_wristWorldPos, _elbowWorldPos, 12f);
            _state = CalibrationState.Calibrated;

            float oneCunCm = cunCalibrator.OneCunInMeters * 100f;
            _hudController?.SetCalibrationState(
                CalibrationPhase.Calibrated,
                $"⚡ 즉시 캘리브레이션!\n1촌 = {oneCunCm:F1}cm\n아래의 혈자리 처방을 확인하세요."
            );

            Debug.Log($"[ARSceneController] 수동 캘리브레이션 완료: 1촌={oneCunCm:F1}cm");

            // 탭 피드백 마커 즉시 소멸 처리
            if (_wristTapMarker != null) Destroy(_wristTapMarker);
            if (_elbowTapMarker != null) Destroy(_elbowTapMarker);

            // 혈자리 정보 카드 표시
            _hudController?.ShowAcupointList(_payload);
        }

        // ── 공간 오디오 ──────────────────────────────────────────────
        private IEnumerator PlaySpatialAudioGuide(List<Vector3> positions)
        {
            if (spatialAudioSource == null || guideBeepClip == null) yield break;
            Camera cam = Camera.main;
            if (cam == null) yield break;

            foreach (var pos in positions)
            {
                Vector3 viewport = cam.WorldToViewportPoint(pos);
                bool visible = viewport.z > 0
                    && viewport.x is > 0 and < 1
                    && viewport.y is > 0 and < 1;

                if (!visible)
                {
                    spatialAudioSource.transform.position = pos;
                    spatialAudioSource.PlayOneShot(guideBeepClip);
                    yield return new WaitForSeconds(2.0f);
                }
            }
        }

        // ── 클린업 ───────────────────────────────────────────────────
        private void ClearAnchors()
        {
            foreach (var a in _spawnedAnchors)
                if (a != null) Destroy(a.gameObject);
            _spawnedAnchors.Clear();
        }

        private void OnDestroy()
        {
            if (deepLinkReceiver != null)
                deepLinkReceiver.OnPayloadReceived -= OnPayloadReceived;
        }
    }
}
