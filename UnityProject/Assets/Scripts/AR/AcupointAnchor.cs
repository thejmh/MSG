using UnityEngine;
using UnityEngine.UI;
using MSG.Models;

namespace MSG.AR
{
    /// <summary>
    /// [WBS 3.5] AcupointAnchor
    /// AR 공간에 배치된 혈자리 마커 오브젝트.
    /// - 3D 구체 마커 (세기별 색상)
    /// - 빌보드 라벨 (혈자리 이름, 번호, 방법 표시)
    /// - 펄싱(Pulsing) 애니메이션으로 시각적 주목성 강화
    /// </summary>
    public class AcupointAnchor : MonoBehaviour
    {
        // ── 마커 데이터 ──────────────────────────────────────────────
        public int AcupointId { get; private set; }
        public int Intensity { get; private set; }   // 0=약하게, 1=강하게
        public int Method { get; private set; }       // 1=누르기, 2=문지르기, 3=두드리기
        public string PointName { get; private set; }
        public string MethodText { get; private set; }

        // ── 색상 정의 ────────────────────────────────────────────────
        private static readonly Color ColorStrong = new Color(0.2f, 0.9f, 0.3f);   // 에메랄드 (강하게)
        private static readonly Color ColorGentle = new Color(0.95f, 0.25f, 0.2f); // 루비 (부드럽게)

        // ── 내부 참조 ────────────────────────────────────────────────
        private GameObject _sphere;
        private GameObject _labelCanvas;
        private Text _labelText;
        private float _pulseTimer;
        private Camera _mainCamera;

        // ── 마커 크기 ────────────────────────────────────────────────
        private const float MARKER_SIZE = 0.025f;      // 2.5cm 구체
        private const float PULSE_SPEED = 2.5f;
        private const float PULSE_SCALE = 0.25f;       // ±25% 펄싱

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            // 펄싱 애니메이션
            _pulseTimer += Time.deltaTime * PULSE_SPEED;
            float pulse = 1f + Mathf.Sin(_pulseTimer) * PULSE_SCALE;
            if (_sphere != null)
                _sphere.transform.localScale = Vector3.one * MARKER_SIZE * pulse;

            // 라벨 빌보드 (카메라를 항상 바라봄)
            if (_labelCanvas != null && _mainCamera != null)
                _labelCanvas.transform.rotation = _mainCamera.transform.rotation;
        }

        /// <summary>
        /// 마커를 초기화하고 혈자리 데이터로 비주얼을 구성합니다.
        /// </summary>
        public void Initialize(int acupointId, int intensity, int method,
            AcupointEntry acupointData, int orderIndex)
        {
            AcupointId = acupointId;
            Intensity = intensity;
            Method = method;

            Color markerColor = intensity == 1 ? ColorStrong : ColorGentle;
            string methodLabel = method switch
            {
                1 => "누르기",
                2 => "문지르기",
                3 => "두드리기",
                _ => "지압"
            };

            if (acupointData != null)
            {
                PointName = acupointData.pointName;
                MethodText = methodLabel;
                BuildMarker(markerColor, $"{orderIndex}. {acupointData.pointName}\n{acupointData.hanja}\n[{methodLabel}]", markerColor);
            }
            else
            {
                PointName = $"혈자리 #{acupointId}";
                MethodText = methodLabel;
                BuildMarker(markerColor, $"{orderIndex}. 혈자리 #{acupointId}\n[{methodLabel}]", markerColor);
            }
        }

        private void BuildMarker(Color color, string label, Color labelColor)
        {
            // ── 1. 3D 구체 마커 ──────────────────────────────────────
            // ARMeshHelper를 사용하여 SphereCollider 추가 및 그에 따른 NullReferenceException 방지
            _sphere = ARMeshHelper.CreateVisualMarker("Sphere", color, MARKER_SIZE, Vector3.zero, true);
            _sphere.transform.SetParent(transform, false);
            _sphere.transform.localPosition = Vector3.zero;

            // ── 2. 빌보드 라벨 (World Space Canvas) ──────────────────
            _labelCanvas = new GameObject("LabelCanvas");
            _labelCanvas.transform.SetParent(transform, false);
            _labelCanvas.transform.localPosition = new Vector3(0.035f, 0.02f, 0f); // 마커 오른쪽 위

            var canvas = _labelCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rectTransform = _labelCanvas.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0.12f, 0.07f);
            rectTransform.localScale = Vector3.one * 0.003f; // 월드 크기 조정

            // 라벨 배경
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_labelCanvas.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.7f); // 반투명 검정 배경

            // 라벨 텍스트
            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(_labelCanvas.transform, false);
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(4, 4);
            txtRect.offsetMax = new Vector2(-4, -4);

            _labelText = txtObj.AddComponent<Text>();
            if (_labelText != null)
            {
                Font safeFont = ARMeshHelper.GetSafeFont();
                if (safeFont != null)
                {
                    _labelText.font = safeFont;
                }
                _labelText.text = label;
                _labelText.color = labelColor;
                _labelText.fontSize = 14;
                _labelText.alignment = TextAnchor.MiddleCenter;
                _labelText.horizontalOverflow = HorizontalWrapMode.Wrap;
                _labelText.verticalOverflow = VerticalWrapMode.Overflow;
                _labelText.fontStyle = FontStyle.Bold;
            }
        }
    }
}
