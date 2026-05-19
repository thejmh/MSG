using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MSG.Models;
using MSG.Services;

namespace MSG.UI
{
    /// <summary>
    /// [WBS 1.5] Dumb Component: 진단 결과 카드 UI 렌더링 전담.
    /// [Instant Layout Rebuild] 결과 씬 전환 시에도 즉시 레이아웃 강제 갱신을 통해 칼 같은 정위치를 보장합니다.
    /// </summary>
    public class ResultCardUI : MonoBehaviour
    {
        [Header("UI 연결 (인스펙터에서 연결)")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Button launchARButton;
        [SerializeField] private Button resetButton;

        private void Awake()
        {
            if (titleText == null) titleText = FindChildSmart<Text>("TitleText");
            if (subtitleText == null) subtitleText = FindChildSmart<Text>("SubtitleText");
            if (launchARButton == null) launchARButton = FindChildSmart<Button>("LaunchARButton");
            if (resetButton == null) resetButton = FindChildSmart<Button>("ResetButton");
        }

        public void Show(DiagnosticResult result)
        {
            gameObject.SetActive(true);
            
            // 1. 텍스트 바인딩 및 폰트 강제 가독성 조절
            if (titleText != null)
            {
                titleText.text = result.title;
                titleText.fontSize = 76; 
                titleText.fontStyle = FontStyle.Bold;
                titleText.color = new Color(0.05f, 0.85f, 0.6f); // Neon Emerald
                titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                titleText.verticalOverflow = VerticalWrapMode.Overflow;
            }

            if (subtitleText != null)
            {
                subtitleText.text = $"✨ 총 {result.pts.Count}개의 혈자리를 안내해드립니다.";
                subtitleText.fontSize = 48; 
                subtitleText.color = new Color(0.85f, 0.87f, 0.90f);
                subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
                subtitleText.verticalOverflow = VerticalWrapMode.Overflow;
            }

            // 2. 결과화면 버튼 폰트 및 큼직한 높이(150px) 보장
            ConfigureButtonText(launchARButton, "마사지 가이드 시작", 46);
            ConfigureButtonText(resetButton, "다시 진단하기", 46);

            // 3. 버튼 이벤트 연결
            if (launchARButton != null)
            {
                launchARButton.onClick.RemoveAllListeners();
                launchARButton.onClick.AddListener(() => HandoffService.Instance.Handoff(result));
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(() =>
                {
                    DiagnosisStateService.Instance.Reset();
                    Hide();
                });
            }

            // 🌟 [정위치 보장 솔루션] 결과 화면 전환 즉시 레이아웃 강제 갱신
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ConfigureButtonText(Button btn, string defaultText, int size)
        {
            if (btn == null) return;
            
            RectTransform btnRect = btn.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                btnRect.sizeDelta = new Vector2(btnRect.sizeDelta.x, 150f); // 💡 결과화면 버튼 높이 150px로 넉넉하게 확장
            }

            Text label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = defaultText;
                label.fontSize = size; 
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.1f, 0.1f, 0.1f);
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        private IEnumerator FadeIn()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / 0.3f);
                yield return null;
            }
            cg.alpha = 1f;
        }

        private T FindChildSmart<T>(string name) where T : Component
        {
            string normalizedTarget = name.Replace(" ", "").Replace("_", "").ToLower();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                string normalizedChild = child.name.Replace(" ", "").Replace("_", "").ToLower();
                if (normalizedChild == normalizedTarget)
                {
                    T comp = child.GetComponent<T>();
                    if (comp != null) return comp;
                }
            }
            Transform backup = transform.Find(name);
            if (backup != null) return backup.GetComponent<T>();
            return null;
        }
    }
}
