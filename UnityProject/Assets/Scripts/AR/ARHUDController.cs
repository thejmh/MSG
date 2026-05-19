using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MSG.Services;
using MSG.Models;

namespace MSG.AR
{
    /// <summary>캘리브레이션 단계 열거형</summary>
    public enum CalibrationPhase
    {
        WaitingForWrist,   // 손목 탭 대기
        WaitingForElbow,   // 팔꿈치 탭 대기
        Calibrated,        // 캘리브레이션 완료
        Done,              // 마커 렌더링 완료
        Error              // 오류
    }

    /// <summary>
    /// [AR HUD] ARHUDController
    /// 터치 기반 AR 앵커링 플로우에 맞춘 단계별 안내 UI.
    /// - 단계 안내 배너 (손목/팔꿈치 탭 유도)
    /// - 캘리브레이션 상태 패널
    /// - 혈자리 정보 카드
    /// - 홈으로 돌아가기 버튼
    /// </summary>
    public class ARHUDController : MonoBehaviour
    {
        private ARSceneController _sceneController;
        private DynamicCunCalibrator _calibrator;

        // ── UI 참조 ──────────────────────────────────────────────────
        private Canvas _hudCanvas;
        private Text _guidanceText;        // 상단 단계 안내 (크고 선명하게)
        private Text _calibStatusText;     // 캘리브레이션 상태
        private GameObject _calibButtonObj;// 즉시 캘리브레이션 버튼
        private Text _acupointsListText;   // 혈자리 목록
        private GameObject _acupointCard;  // 혈자리 정보 카드 (숨김/표시)
        private GameObject _instructionsOverlay; // 시작 가이드 오버레이

        /// <summary>시작 가이드 오버레이가 화면에 표시 중인지 여부</summary>
        public bool IsInstructionPopupActive => _instructionsOverlay != null && _instructionsOverlay.activeSelf;

        private void Awake()
        {
            _sceneController = FindFirstObjectByType<ARSceneController>();
            _calibrator = FindFirstObjectByType<DynamicCunCalibrator>();
        }

        private void Start()
        {
            CreateHUDCanvas();
        }

        // ── 외부 API ─────────────────────────────────────────────────

        /// <summary>캘리브레이션 단계 및 안내 텍스트를 업데이트.</summary>
        public void SetCalibrationState(CalibrationPhase phase, string message)
        {
            if (_guidanceText != null)
                _guidanceText.text = message;

            if (_calibStatusText != null)
            {
                _calibStatusText.text = phase switch
                {
                    CalibrationPhase.WaitingForWrist => "🔵 1단계: 손목 위치 지정 대기 중",
                    CalibrationPhase.WaitingForElbow => "🟡 2단계: 팔꿈치 위치 지정 대기 중",
                    CalibrationPhase.Calibrated       => "🟢 캘리브레이션 완료 — 혈자리 계산 중",
                    CalibrationPhase.Done             => "✅ 혈자리 표시 완료",
                    CalibrationPhase.Error            => "⚠️ 오류 — 재시도 중",
                    _ => ""
                };

                _calibStatusText.color = phase switch
                {
                    CalibrationPhase.Done      => new Color(0.2f, 0.9f, 0.5f),
                    CalibrationPhase.Calibrated => new Color(0.2f, 0.9f, 0.5f),
                    CalibrationPhase.Error     => new Color(1f, 0.3f, 0.3f),
                    _                          => new Color(0.95f, 0.85f, 0.2f)
                };
            }

            // 즉시 캘리브레이션 버튼: 렌더링 완료 후 숨김
            if (_calibButtonObj != null)
                _calibButtonObj.SetActive(phase == CalibrationPhase.WaitingForWrist ||
                                          phase == CalibrationPhase.WaitingForElbow ||
                                          phase == CalibrationPhase.Error);

            // 혈자리 카드: 캘리브레이션 완료 시점에 브리핑 표시
            if (_acupointCard != null)
                _acupointCard.SetActive(phase == CalibrationPhase.Calibrated);
        }

        /// <summary>혈자리 목록을 카드에 표시.</summary>
        public void ShowAcupointList(HandoffPayload payload)
        {
            if (_acupointsListText == null || payload == null) return;

            var sb = new System.Text.StringBuilder();
            int idx = 1;
            foreach (var pt in payload.pts)
            {
                AcupointEntry data = DataFetchService.Instance?.GetAcupoint(pt.id);
                string name = data != null ? $"{data.pointName}({data.hanja})" : $"ID:{pt.id}";
                string intensity = pt.i == 1
                    ? "<color=#50FF80>[강하게]</color>"
                    : "<color=#FF6060>[부드럽게]</color>";
                string method = pt.m switch
                {
                    1 => "누르기",
                    2 => "문지르기",
                    3 => "두드리기",
                    _ => "지압"
                };
                float cunOffset = DynamicCunCalibrator.Instance?.GetCunOffset(pt.id) ?? 0f;
                string offsetStr = cunOffset >= 0 ? $"+{cunOffset:F1}촌" : $"{cunOffset:F1}촌";

                sb.AppendLine($"{idx}. <b>{name}</b>  {intensity}");
                sb.AppendLine($"   손목기준 {offsetStr} | {method}");
                if (data != null) sb.AppendLine($"   {data.location}");
                sb.AppendLine();
                idx++;
            }
            _acupointsListText.text = sb.ToString();
        }

        // ── Canvas 구성 ───────────────────────────────────────────────
        private void CreateHUDCanvas()
        {
            // ── EventSystem 자동 생성 (UI 클릭 입력 처리용) ──────
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[ARHUDController] EventSystem 자동 생성 완료.");
            }

            var canvasGo = new GameObject("AR_HUD_Canvas");
            canvasGo.transform.SetParent(transform);

            _hudCanvas = canvasGo.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 10;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // ── 1. 상단 안내 배너 (백그라운드 이미지) ──────────────────
            var guidanceBanner = CreatePanel(canvasGo.transform,
                new Vector2(0f, 0.88f), new Vector2(1f, 1f));
            AddBackground(guidanceBanner, new Color(0f, 0f, 0f, 0.72f));
            
            // 텍스트 전용 자식 오브젝트 생성 (Image와 Text가 동일 오브젝트에서 충돌하는 문제 우회)
            var guidanceTextGo = CreatePanel(guidanceBanner.transform, Vector2.zero, Vector2.one);
            _guidanceText = AddText(guidanceTextGo,
                "📍 손목 안쪽을 탭하세요", 42,
                Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);

            // ── 2. 캘리브레이션 상태 패널 (백그라운드 이미지) ──────────
            var calibPanel = CreatePanel(canvasGo.transform,
                new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.87f));
            AddBackground(calibPanel, new Color(0.08f, 0.12f, 0.18f, 0.88f));
            
            // 텍스트 전용 자식 오브젝트 생성
            var calibTextGo = CreatePanel(calibPanel.transform, Vector2.zero, Vector2.one);
            _calibStatusText = AddText(calibTextGo,
                "🔵 1단계: 손목 위치 지정 대기 중",
                34, new Color(0.95f, 0.85f, 0.2f), FontStyle.Normal);

            // ── 3. 즉시 캘리브레이션 버튼 (에디터/테스트용) ──────
            _calibButtonObj = CreateButton(canvasGo.transform,
                "CalibBtn", "⚡ 즉시 캘리브레이션 (테스트용)",
                new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.75f),
                () => _sceneController?.TriggerManualCalibration());
            var btnImg = _calibButtonObj.GetComponent<Image>();
            if (btnImg != null) btnImg.color = new Color(0.1f, 0.45f, 0.9f);

            // ── 4. 혈자리 정보 카드 (처음엔 숨김) ─────────────────
            _acupointCard = CreatePanel(canvasGo.transform,
                new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.70f));
            AddBackground(_acupointCard, new Color(0.04f, 0.06f, 0.12f, 0.94f));

            var cardHeader = CreatePanel(_acupointCard.transform,
                new Vector2(0f, 0.88f), new Vector2(1f, 1f));
            AddText(cardHeader, "📝 처방 혈자리 가이드", 36,
                new Color(0.2f, 0.85f, 0.6f), FontStyle.Bold);

            var listArea = CreatePanel(_acupointCard.transform,
                new Vector2(0f, 0.18f), new Vector2(1f, 0.87f));
            _acupointsListText = AddText(listArea, "", 32,
                new Color(0.88f, 0.9f, 0.95f), FontStyle.Normal, TextAnchor.UpperLeft);

            // 혈자리 브리핑 하단 확인 버튼 추가
            var confirmBtn = CreateButton(_acupointCard.transform,
                "ConfirmBtn", "확인 및 3D 혈자리 표시",
                new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.15f),
                () =>
                {
                    if (_sceneController != null)
                    {
                        _sceneController.RenderAcupointMarkers();
                    }
                    if (_acupointCard != null)
                    {
                        _acupointCard.SetActive(false);
                    }
                });
            var confirmBtnImg = confirmBtn.GetComponent<Image>();
            if (confirmBtnImg != null) confirmBtnImg.color = new Color(0.15f, 0.68f, 0.37f); // 녹색

            _acupointCard.SetActive(false);

            // ── 5. 하단 버튼 영역 ──────────────────────────────────
            // 5-1. 재보정 버튼
            var resetBtn = CreateButton(canvasGo.transform,
                "ResetBtn", "🔄 다시 맞추기 (재보정)",
                new Vector2(0.06f, 0.03f), new Vector2(0.48f, 0.11f),
                () =>
                {
                    if (_sceneController != null)
                    {
                        _sceneController.BeginCalibration();
                    }
                });
            var resetBtnImg = resetBtn.GetComponent<Image>();
            if (resetBtnImg != null) resetBtnImg.color = new Color(0.85f, 0.35f, 0.2f);
            var resetBtnTxt = resetBtn.GetComponentInChildren<Text>();
            if (resetBtnTxt != null) resetBtnTxt.color = Color.white;

            // 5-2. 진단 화면으로 복귀 버튼
            var homeBtn = CreateButton(canvasGo.transform,
                "HomeBtn", "↩️ 진단 화면으로",
                new Vector2(0.52f, 0.03f), new Vector2(0.94f, 0.11f),
                () =>
                {
                    if (HandoffData.Instance != null) HandoffData.Instance.payload = null;
                    SceneManager.LoadScene("DiagnosisScene");
                });
            var homeBtnImg = homeBtn.GetComponent<Image>();
            if (homeBtnImg != null) homeBtnImg.color = new Color(0.22f, 0.26f, 0.34f);
            var homeBtnTxt = homeBtn.GetComponentInChildren<Text>();
            if (homeBtnTxt != null) homeBtnTxt.color = Color.white;

            // ── 6. 시작 가이드 오버레이 (최상단) ────────────────────
            _instructionsOverlay = CreatePanel(canvasGo.transform,
                Vector2.zero, Vector2.one);
            AddBackground(_instructionsOverlay, new Color(0.04f, 0.06f, 0.12f, 0.98f)); // 어두운 반투명 배경

            // 중앙 카드 컨테이너
            var cardGo = CreatePanel(_instructionsOverlay.transform,
                new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.85f));
            AddBackground(cardGo, new Color(0.08f, 0.12f, 0.22f, 0.95f));

            // 카드 테두리 장식 또는 헤더
            var headerGo = CreatePanel(cardGo.transform,
                new Vector2(0f, 0.88f), new Vector2(1f, 1f));
            AddText(headerGo, "👋 혈자리 AR 매핑 가이드", 44, new Color(0.2f, 0.9f, 0.6f), FontStyle.Bold);

            // 가이드 상세 텍스트
            var bodyGo = CreatePanel(cardGo.transform,
                new Vector2(0f, 0.2f), new Vector2(1f, 0.86f));
            
            string instructionsText =
                "본 앱은 증강현실(AR) 기술로 개인별 신체 촌도(Cun)를 측정하여 혈자리를 표시합니다.\n\n" +
                "<b>[측정 절차]</b>\n" +
                "1. 화면의 안내를 확인하세요.\n" +
                "2. <b>손목 안쪽 주름의 중앙</b>을 화면에서 터치하세요.\n" +
                "3. <b>팔꿈치 안쪽의 접히는 주름 끝</b>을 터치하세요.\n" +
                "4. 1촌 척도가 계산되어 팔 위에 혈자리가 마킹됩니다.\n\n" +
                "<b>⚠️ 필독 주의사항</b>\n" +
                "• <b>측정 도중 팔을 움직이지 말고 고정</b>하십시오.\n" +
                "• 팔을 움직여 마커 위치가 틀어진 경우, 하단의 <b>'🔄 다시 맞추기'</b> 버튼을 눌러 언제든 재측정할 수 있습니다.";
            
            AddText(bodyGo, instructionsText, 32, new Color(0.85f, 0.88f, 0.94f), FontStyle.Normal, TextAnchor.UpperLeft);

            // 시작하기 버튼
            var startBtn = CreateButton(cardGo.transform,
                "StartBtn", "안내 확인 및 시작하기",
                new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.17f),
                () =>
                {
                    _instructionsOverlay.SetActive(false);
                });
            var startBtnImg = startBtn.GetComponent<Image>();
            if (startBtnImg != null) startBtnImg.color = new Color(0.15f, 0.68f, 0.37f);
            var startBtnTxt = startBtn.GetComponentInChildren<Text>();
            if (startBtnTxt != null)
            {
                startBtnTxt.fontSize = 34;
                startBtnTxt.color = Color.white;
            }
        }

        // ── UI 헬퍼 ──────────────────────────────────────────────────
        private GameObject CreatePanel(Transform parent, Vector2 ancMin, Vector2 ancMax)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            SetAnchors(r, ancMin, ancMax);
            return go;
        }

        private void AddBackground(GameObject obj, Color color)
        {
            if (obj == null) return;
            var img = obj.AddComponent<Image>();
            img.color = color;
            img.material = null;
        }

        private Text AddText(GameObject obj, string content, int fontSize,
            Color color, FontStyle style,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            if (obj == null) return null;

            var text = obj.GetComponent<Text>();
            if (text == null) text = obj.AddComponent<Text>();

            if (text == null)
            {
                Debug.LogError("[ARHUDController] AddComponent<Text> returned null!");
                return null;
            }

            Font safeFont = ARMeshHelper.GetSafeFont();
            if (safeFont != null)
            {
                text.font = safeFont;
            }
            else
            {
                Debug.LogWarning("[ARHUDController] GetSafeFont returned null, fallback is active.");
            }

            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.alignment = anchor;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var r = obj.GetComponent<RectTransform>();
            if (r != null)
            {
                r.offsetMin = new Vector2(8, 8);
                r.offsetMax = new Vector2(-8, -8);
            }
            return text;
        }

        private GameObject CreateButton(Transform parent, string name, string label,
            Vector2 ancMin, Vector2 ancMax, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            SetAnchors(rect, ancMin, ancMax);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.65f, 0.4f);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            AddText(labelGo, label, 36, Color.white, FontStyle.Bold);

            return go;
        }

        private void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
