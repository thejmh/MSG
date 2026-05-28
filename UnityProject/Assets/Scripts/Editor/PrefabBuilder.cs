#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [MSG Prefab Builder] OptionButton 프리팹 자동 생성.
/// Unity 메뉴 MSG > Build OptionButton Prefab 실행.
/// </summary>
public static class PrefabBuilder
{
    [MenuItem("MSG/Build OptionButton Prefab")]
    public static void BuildOptionButtonPrefab()
    {
        string prefabPath = "Assets/OptionButton.prefab";

        // ── 버튼 루트 오브젝트 ────────────────────────────────────
        var btnGo = new GameObject("OptionButton");

        var rect = btnGo.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(960f, 140f);

        // 버튼 배경 이미지
        var img = btnGo.AddComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.30f, 1f);

        // Button 컴포넌트
        var btn = btnGo.AddComponent<Button>();

        // ColorBlock 설정 (터치 피드백)
        var colors = btn.colors;
        colors.normalColor      = new Color(0.18f, 0.22f, 0.30f, 1f);
        colors.highlightedColor = new Color(0.22f, 0.55f, 0.40f, 1f);
        colors.pressedColor     = new Color(0.15f, 0.68f, 0.37f, 1f);
        colors.selectedColor    = new Color(0.20f, 0.50f, 0.38f, 1f);
        colors.disabledColor    = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.fadeDuration     = 0.1f;
        btn.colors = colors;

        // LayoutElement (QuestionCardUI에서 preferredHeight 제어용)
        var layoutElement = btnGo.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 140f;
        layoutElement.flexibleWidth = 1f;

        // ── 텍스트 자식 오브젝트 ─────────────────────────────────
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(btnGo.transform, false);

        var labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.05f, 0.05f);
        labelRect.anchorMax = new Vector2(0.95f, 0.95f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var text = labelGo.AddComponent<Text>();
        text.text = "옵션";
        text.fontSize = 46;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.92f, 0.93f, 0.96f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;

        // 폰트 설정
        Font font = Resources.Load<Font>("LiberationSans");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null) text.font = font;

        // ── 프리팹 저장 ───────────────────────────────────────────
        bool success;
        var prefab = PrefabUtility.SaveAsPrefabAsset(btnGo, prefabPath, out success);
        Object.DestroyImmediate(btnGo);

        if (success)
            Debug.Log($"[PrefabBuilder] ✅ OptionButton.prefab 생성 완료: {prefabPath}");
        else
            Debug.LogError("[PrefabBuilder] OptionButton.prefab 생성 실패.");

        AssetDatabase.Refresh();
    }
}
#endif
