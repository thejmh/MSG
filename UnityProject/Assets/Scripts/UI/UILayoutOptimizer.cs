using UnityEngine;
using UnityEngine.UI;

namespace MSG.UI
{
    /// <summary>
    /// [Aesthetics & Responsive] UI Layout Optimizer
    /// [Flat UI & Dynamic Sizing Perfect - sc_ui2p/3 해결 에디션] 🚀
    /// 1. 스크린샷 피드백 반영(sc_ui3.png): 결과 화면의 텍스트가 포개지는 현상을 방지하기 위해 
    ///    childControlHeight를 true로 세팅하여 레이아웃 그룹이 자식들의 권장 높이(LayoutElement)를 완벽 강제 통제하게 만듭니다.
    /// 2. 초기 문구 위치 세부 튜닝: 중심 앵커를 0.45f ➡️ 0.44f로 변경하여 문구를 눈맛 좋게 10p 수준 하향 안착시킵니다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    public class UILayoutOptimizer : MonoBehaviour
    {
        [Header("디자인 시스템 설정")]
        public Vector2 referenceResolution = new Vector2(1080, 1920);
        [Range(0, 1)] public float matchWidthOrHeight = 0.5f;

        [Header("글로벌 폰트 파일 (TTF/OTF)")]
        public Font customFont;

        [Header("버튼 및 레이아웃 규격 (여유로운 Spacing)")]
        public float buttonHeight = 140f;
        public float buttonSpacing = 42f;       
        public float cardVerticalSpacing = 120f; 

        [ContextMenu("✨ UI 즉시 자동 정렬 및 최적화")]
        public void OptimizeLayout()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) return;

            // 1. Canvas Scaler 모던 세팅 (반응형 화면 대응)
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;

            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

            // 2. 풀스크린 배경 패널 자동 배치 및 생성
            Transform bgPanel = FindChildSmart(transform, "Background");
            if (bgPanel == null)
            {
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(transform);
                bgObj.transform.SetAsFirstSibling();
                bgPanel = bgObj.transform;
            }
            
            OptimizeElement(bgPanel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImage = bgPanel.GetComponent<Image>();
            if (bgImage == null) bgImage = bgPanel.gameObject.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.15f, 0.20f, 1.0f); 

            // 3. LoadingPanel 정렬
            OptimizePanel("LoadingPanel", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            // 4. ProgressBar 영구 비활성화
            Transform progressBar = FindChildSmart(transform, "ProgressBar");
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false); 
            }

            // 5. 🌟 QuestionCard (Notch Safe Area 미세 조정: 0.45 ➡️ 0.44 하향 안착) 🌟
            Transform questionCard = FindChildSmart(transform, "QuestionCard");
            if (questionCard != null)
            {
                RectTransform cardRect = questionCard.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    // 💡 세로축 중심을 0.44f로 낮춰 문구를 딱 10포인트 가량 보기 좋게 아래로 안착!
                    cardRect.anchorMin = new Vector2(0.06f, 0.44f);
                    cardRect.anchorMax = new Vector2(0.94f, 0.44f);
                    cardRect.pivot = new Vector2(0.5f, 0.5f);
                    cardRect.offsetMin = new Vector2(0, cardRect.offsetMin.y);
                    cardRect.offsetMax = new Vector2(0, cardRect.offsetMax.y);
                }

                Image bg = questionCard.GetComponent<Image>();
                if (bg != null) bg.color = new Color(0f, 0f, 0f, 0f);
                Shadow shadow = questionCard.GetComponent<Shadow>();
                if (shadow != null) DestroyImmediate(shadow);

                VerticalLayoutGroup cardLayout = questionCard.GetComponent<VerticalLayoutGroup>();
                if (cardLayout == null) cardLayout = questionCard.gameObject.AddComponent<VerticalLayoutGroup>();
                cardLayout.padding = new RectOffset(45, 45, 60, 60); 
                cardLayout.spacing = cardVerticalSpacing; 
                cardLayout.childAlignment = TextAnchor.UpperCenter;
                cardLayout.childControlWidth = true;
                // 💡 [PreferredHeight 정밀 통제] 텍스트 및 자식들의 고유 높이 관리를 100% 보장
                cardLayout.childControlHeight = true; 
                cardLayout.childForceExpandWidth = true;
                cardLayout.childForceExpandHeight = false;

                ContentSizeFitter cardFitter = questionCard.GetComponent<ContentSizeFitter>();
                if (cardFitter == null) cardFitter = questionCard.gameObject.AddComponent<ContentSizeFitter>();
                cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // 5-1. QuestionText 세팅 (LayoutElement 높이 280px)
                Transform qTextTrans = FindChildSmart(questionCard, "QuestionText");
                if (qTextTrans != null)
                {
                    qTextTrans.SetAsFirstSibling();

                    Text qText = qTextTrans.GetComponent<Text>();
                    if (qText != null)
                    {
                        qText.alignment = TextAnchor.MiddleCenter;
                        qText.fontSize = 68;
                        qText.fontStyle = FontStyle.Bold;
                        qText.color = new Color(0.96f, 0.96f, 0.98f, 1f); 
                        qText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        qText.verticalOverflow = VerticalWrapMode.Overflow;
                        qText.lineSpacing = 1.25f;
                    }

                    LayoutElement textLayout = qTextTrans.GetComponent<LayoutElement>();
                    if (textLayout == null) textLayout = qTextTrans.gameObject.AddComponent<LayoutElement>();
                    textLayout.preferredHeight = 280f; 
                }

                // 5-2. OptionsContainer 세팅
                Transform optionsTrans = FindChildSmart(questionCard, "OptionsContainer");
                if (optionsTrans != null)
                {
                    optionsTrans.SetAsLastSibling();

                    VerticalLayoutGroup optionsLayout = optionsTrans.GetComponent<VerticalLayoutGroup>();
                    if (optionsLayout == null) optionsLayout = optionsTrans.gameObject.AddComponent<VerticalLayoutGroup>();
                    optionsLayout.spacing = buttonSpacing; 
                    optionsLayout.childAlignment = TextAnchor.UpperCenter;
                    optionsLayout.childControlWidth = true;
                    optionsLayout.childControlHeight = true; // 버튼 preferredHeight(140f) 통제
                    optionsLayout.childForceExpandWidth = true;
                    optionsLayout.childForceExpandHeight = false;
                    optionsLayout.padding = new RectOffset(10, 10, 10, 10);

                    ContentSizeFitter optionsFitter = optionsTrans.GetComponent<ContentSizeFitter>();
                    if (optionsFitter == null) optionsFitter = optionsTrans.gameObject.AddComponent<ContentSizeFitter>();
                    optionsFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }

            // 6. 🌟 ResultCard (Notch Safe Area 0.44 하향 및 겹침 방지 childControlHeight=true 지정) 🌟
            Transform resultCard = FindChildSmart(transform, "ResultCard");
            if (resultCard != null)
            {
                RectTransform cardRect = resultCard.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.anchorMin = new Vector2(0.06f, 0.44f);
                    cardRect.anchorMax = new Vector2(0.94f, 0.44f);
                    cardRect.pivot = new Vector2(0.5f, 0.5f);
                    cardRect.offsetMin = new Vector2(0, cardRect.offsetMin.y);
                    cardRect.offsetMax = new Vector2(0, cardRect.offsetMax.y);
                }

                Image bg = resultCard.GetComponent<Image>();
                if (bg != null) bg.color = new Color(0f, 0f, 0f, 0f);
                Shadow shadow = resultCard.GetComponent<Shadow>();
                if (shadow != null) DestroyImmediate(shadow);

                VerticalLayoutGroup cardLayout = resultCard.GetComponent<VerticalLayoutGroup>();
                if (cardLayout == null) cardLayout = resultCard.gameObject.AddComponent<VerticalLayoutGroup>();
                cardLayout.padding = new RectOffset(45, 45, 60, 60);
                cardLayout.spacing = 90f; 
                cardLayout.childAlignment = TextAnchor.UpperCenter;
                cardLayout.childControlWidth = true;
                // 💡 [sc_ui3.png 해답] childControlHeight를 true로 변경하여 타이틀과 서브텍스트 높이를 완벽 격리! 겹침 방지!
                cardLayout.childControlHeight = true; 
                cardLayout.childForceExpandWidth = true;
                cardLayout.childForceExpandHeight = false;

                ContentSizeFitter cardFitter = resultCard.GetComponent<ContentSizeFitter>();
                if (cardFitter == null) cardFitter = resultCard.gameObject.AddComponent<ContentSizeFitter>();
                cardFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // TitleText
                Transform tTextTrans = FindChildSmart(resultCard, "TitleText");
                if (tTextTrans != null)
                {
                    tTextTrans.SetAsFirstSibling();
                    Text tText = tTextTrans.GetComponent<Text>();
                    if (tText != null)
                    {
                        tText.alignment = TextAnchor.MiddleCenter;
                        tText.fontSize = 76; 
                        tText.fontStyle = FontStyle.Bold;
                        tText.color = new Color(0.05f, 0.85f, 0.6f); 
                        tText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        tText.verticalOverflow = VerticalWrapMode.Overflow;
                    }
                    LayoutElement tLayout = tTextTrans.GetComponent<LayoutElement>();
                    if (tLayout == null) tLayout = tTextTrans.gameObject.AddComponent<LayoutElement>();
                    tLayout.preferredHeight = 180f; 
                }

                // SubtitleText
                Transform subTextTrans = FindChildSmart(resultCard, "SubtitleText");
                if (subTextTrans != null)
                {
                    Text subText = subTextTrans.GetComponent<Text>();
                    if (subText != null)
                    {
                        subText.alignment = TextAnchor.MiddleCenter;
                        subText.fontSize = 48; 
                        subText.color = new Color(0.85f, 0.87f, 0.90f); 
                        subText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        subText.verticalOverflow = VerticalWrapMode.Overflow;
                        subText.lineSpacing = 1.15f;
                    }
                    LayoutElement subLayout = subTextTrans.GetComponent<LayoutElement>();
                    if (subLayout == null) subLayout = subTextTrans.gameObject.AddComponent<LayoutElement>();
                    subLayout.preferredHeight = 150f; 
                }

                // Buttons
                ConfigureResultButtonLayout(FindChildSmart(resultCard, "LaunchARButton"));
                ConfigureResultButtonLayout(FindChildSmart(resultCard, "ResetButton"));
            }

            // 7. 공통 UI 정렬 (BackButton)
            Transform backBtnTrans = FindChildSmart(transform, "BackButton");
            OptimizeElement(backBtnTrans, 
                new Vector2(0.05f, 0.89f), new Vector2(0.28f, 0.95f), 
                Vector2.zero, Vector2.zero);

            // 8. 글로벌 폰트 및 가독성 크기 일괄 적용
            ApplyGlobalFontScaling();

            // 캔버스 동기식 강제 리빌드
            Canvas.ForceUpdateCanvases();
            if (questionCard != null) LayoutRebuilder.ForceRebuildLayoutImmediate(questionCard.GetComponent<RectTransform>());
            if (resultCard != null) LayoutRebuilder.ForceRebuildLayoutImmediate(resultCard.GetComponent<RectTransform>());

            Debug.Log("✨ [UILayoutOptimizer] sc_ui3 겹침 해결 및 0.44f 하향 적용 완벽 갱신!");
        }

        private void ConfigureResultButtonLayout(Transform btn)
        {
            if (btn == null) return;
            LayoutElement layout = btn.GetComponent<LayoutElement>();
            if (layout == null) layout = btn.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 150f; 
            layout.preferredWidth = 800f;
        }

        private void Start()
        {
            OptimizeLayout();
        }

        private void OptimizePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            Transform t = FindChildSmart(transform, name);
            OptimizeElement(t, anchorMin, anchorMax, offsetMin, offsetMax);
        }

        private void OptimizeElement(Transform t, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (t == null) return;
            RectTransform rect = t.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private Transform FindChildSmart(Transform parent, string targetName)
        {
            if (parent == null) return null;
            string normalizedTarget = targetName.Replace(" ", "").Replace("_", "").ToLower();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string normalizedChild = child.name.Replace(" ", "").Replace("_", "").ToLower();
                if (normalizedChild == normalizedTarget)
                {
                    return child;
                }
            }
            return parent.Find(targetName);
        }

        private void ApplyGlobalFontScaling()
        {
            Text[] allTexts = GetComponentsInChildren<Text>(true);
            foreach (Text txt in allTexts)
            {
                if (customFont != null)
                {
                    txt.font = customFont;
                }

                string objName = txt.gameObject.name.ToLower();

                if (objName.Contains("questiontext") || objName.Contains("titletext")) continue;

                txt.horizontalOverflow = HorizontalWrapMode.Wrap;
                txt.verticalOverflow = VerticalWrapMode.Overflow;

                if (objName.Contains("btn") || objName.Contains("button") || txt.transform.parent.name.ToLower().Contains("button"))
                {
                    txt.fontSize = 46; 
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.color = new Color(0.12f, 0.12f, 0.12f);
                }
                else if (objName.Contains("subtitle") || objName.Contains("desc"))
                {
                    txt.fontSize = 48;
                    txt.alignment = TextAnchor.MiddleCenter;
                }
                else if (objName.Contains("loading"))
                {
                    txt.fontSize = 55;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                }
                else
                {
                    if (txt.fontSize < 38)
                    {
                        txt.fontSize = 38;
                    }
                }
            }
        }
    }
}
