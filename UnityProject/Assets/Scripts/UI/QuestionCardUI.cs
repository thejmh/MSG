using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MSG.Models;
using MSG.Services;

namespace MSG.UI
{
    /// <summary>
    /// [WBS 1.5] Dumb Component: 질문 카드 UI 렌더링 전담.
    /// [Layout Lifecycle Fix] 
    /// 이전 버튼을 파괴할 때 즉시 부모 관계를 단절하여 유니티 UI 생명주기 정렬 지연 버그(sc_ui2p.png)를 해결합니다.
    /// </summary>
    public class QuestionCardUI : MonoBehaviour
    {
        [Header("UI 연결 (인스펙터에서 연결)")]
        [SerializeField] private Text questionText;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

        public void Show(TreeNode node)
        {
            gameObject.SetActive(true);
            
            if (questionText != null)
            {
                questionText.text = node.text;
                questionText.horizontalOverflow = HorizontalWrapMode.Wrap;
                questionText.verticalOverflow = VerticalWrapMode.Overflow;
                questionText.lineSpacing = 1.25f; 
            }

            RenderOptions(node.options);

            // 🌟 레이아웃 동기식 갱신으로 겹침 완벽 차단
            Canvas.ForceUpdateCanvases();
            if (optionsContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(optionsContainer.GetComponent<RectTransform>());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RenderOptions(List<TreeOption> options)
        {
            // 💡 [유니티 UI 버그 원천 봉쇄]
            // Destroy(btn)은 비동기 파괴이므로 부모관계를 먼저 끊어(SetParent(null)) 
            // 레이아웃 그룹의 계산 범위에서 '즉시' 제외되도록 조치합니다.
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null)
                {
                    btn.transform.SetParent(null);
                    Destroy(btn);
                }
            }
            _spawnedButtons.Clear();

            foreach (var opt in options)
            {
                var btnObj = Instantiate(optionButtonPrefab, optionsContainer);
                _spawnedButtons.Add(btnObj);

                // 1. 버튼 크기 및 RectTransform 세팅
                RectTransform rect = btnObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, 140f);
                }

                // 💡 [PreferredHeight 연동] 레이아웃 그룹의 childControlHeight 통제를 받도록 세팅
                LayoutElement elementLayout = btnObj.GetComponent<LayoutElement>();
                if (elementLayout == null) elementLayout = btnObj.AddComponent<LayoutElement>();
                elementLayout.preferredHeight = 140f;

                // 2. 버튼 텍스트의 크기 및 속성 최적화
                Text label = btnObj.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = opt.text;
                    label.fontSize = 46; 
                    label.fontStyle = FontStyle.Bold;
                    label.alignment = TextAnchor.MiddleCenter;
                    label.color = new Color(0.1f, 0.1f, 0.1f);
                    
                    label.horizontalOverflow = HorizontalWrapMode.Wrap;
                    label.verticalOverflow = VerticalWrapMode.Overflow;
                    
                    RectTransform labelRect = label.GetComponent<RectTransform>();
                    if (labelRect != null)
                    {
                        labelRect.anchorMin = new Vector2(0.05f, 0.05f);
                        labelRect.anchorMax = new Vector2(0.95f, 0.95f);
                        labelRect.offsetMin = Vector2.zero;
                        labelRect.offsetMax = Vector2.zero;
                    }
                }

                var btn = btnObj.GetComponent<Button>();
                string nextId = opt.nextId;
                btn.onClick.AddListener(() =>
                    DiagnosisStateService.Instance.SelectOption(nextId));
            }
        }

        private IEnumerator FadeIn()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / 0.2f);
                yield return null;
            }
            cg.alpha = 1f;
        }
    }
}
