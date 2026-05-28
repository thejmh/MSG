#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MSG.UI;
using MSG.Services;
using MSG.AR;
using UnityEngine.InputSystem.UI;

/// <summary>
/// [MSG Scene Builder] 에디터 전용 씬 자동 구성 도구.
/// Unity 메뉴 MSG > Build Scenes 를 실행하면 DiagnosisScene과 ARScene을
/// 코드로 완전히 재구성합니다. 인스펙터 수동 와이어링 불필요.
/// </summary>
public static class SceneBuilder
{
    // ─────────────────────────────────────────────────────────────
    //  진단 씬 빌드
    // ─────────────────────────────────────────────────────────────
    [MenuItem("MSG/Build DiagnosisScene")]
    public static void BuildDiagnosisScene()
    {
        // 씬 생성 또는 열기
        string scenePath = "Assets/Scenes/DiagnosisScene.unity";
        EnsureSceneDirectoryExists();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── 1. 필수 서비스 오브젝트 ──────────────────────────────
        CreateServiceObject<DataFetchService>("DataFetchService");
        CreateServiceObject<DiagnosisStateService>("DiagnosisStateService");
        CreateServiceObject<HandoffService>("HandoffService");
        CreateServiceObject<FallbackRouter>("FallbackRouter");
        CreateServiceObject<HandoffData>("HandoffData");

        // ── 2. EventSystem ────────────────────────────────────────
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();

        // ── 3. Canvas (메인 UI) ───────────────────────────────────
        var canvasGo = new GameObject("DiagnosisCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // UILayoutOptimizer 부착 (씬 시작 시 자동 레이아웃 최적화)
        var optimizer = canvasGo.AddComponent<UILayoutOptimizer>();

        // ── 4. 배경 패널 ──────────────────────────────────────────
        var bgGo = CreateUIPanel(canvasGo.transform, "Background",
            Vector2.zero, Vector2.one);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.15f, 0.20f, 1f);

        // ── 5. LoadingPanel ───────────────────────────────────────
        var loadingGo = CreateUIPanel(canvasGo.transform, "LoadingPanel",
            Vector2.zero, Vector2.one);
        loadingGo.AddComponent<LoadingUI>();
        var loadingText = CreateText(loadingGo.transform, "LoadingText",
            "데이터를 불러오는 중...", 55, Color.white, FontStyle.Bold);
        SetFullStretch(loadingText.GetComponent<RectTransform>());

        // ── 6. ProgressBar ────────────────────────────────────────
        var progressBarGo = CreateUIPanel(canvasGo.transform, "ProgressBar",
            new Vector2(0f, 0.93f), new Vector2(1f, 0.97f));
        progressBarGo.AddComponent<ProgressBarUI>();
        var progressBarBg = progressBarGo.AddComponent<Image>();
        progressBarBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        var fillGo = CreateUIPanel(progressBarGo.transform, "Fill",
            Vector2.zero, new Vector2(0f, 1f)); // fillAmount=0 초기값
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.5f, 1f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 0f;

        // ── 7. BackButton ─────────────────────────────────────────
        var backBtnGo = CreateUIPanel(canvasGo.transform, "BackButton",
            new Vector2(0.05f, 0.89f), new Vector2(0.28f, 0.95f));
        var backBtnImg = backBtnGo.AddComponent<Image>();
        backBtnImg.color = new Color(0.3f, 0.35f, 0.45f, 0.9f);
        var backBtn = backBtnGo.AddComponent<Button>();
        var backBtnText = CreateText(backBtnGo.transform, "Label",
            "◀ 뒤로", 42, Color.white, FontStyle.Bold);
        SetFullStretch(backBtnText.GetComponent<RectTransform>());

        // ── 8. QuestionCard ───────────────────────────────────────
        var questionCardGo = CreateUIPanel(canvasGo.transform, "QuestionCard",
            new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.88f));
        questionCardGo.AddComponent<CanvasGroup>();
        var questionCardUI = questionCardGo.AddComponent<QuestionCardUI>();

        // QuestionText
        var questionTextGo = CreateText(questionCardGo.transform, "QuestionText",
            "질문을 불러오는 중...", 68, new Color(0.96f, 0.96f, 0.98f), FontStyle.Bold);
        var questionTextRect = questionTextGo.GetComponent<RectTransform>();
        questionTextRect.anchorMin = new Vector2(0f, 0.65f);
        questionTextRect.anchorMax = new Vector2(1f, 1f);
        questionTextRect.offsetMin = new Vector2(20f, 0f);
        questionTextRect.offsetMax = new Vector2(-20f, 0f);
        var questionTextComp = questionTextGo.GetComponent<Text>();
        questionTextComp.alignment = TextAnchor.MiddleCenter;
        questionTextComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        questionTextComp.verticalOverflow = VerticalWrapMode.Overflow;
        questionTextComp.lineSpacing = 1.25f;

        // OptionsContainer
        var optionsContainerGo = CreateUIPanel(questionCardGo.transform, "OptionsContainer",
            new Vector2(0f, 0f), new Vector2(1f, 0.62f));
        var optionsLayout = optionsContainerGo.AddComponent<VerticalLayoutGroup>();
        optionsLayout.spacing = 42f;
        optionsLayout.childAlignment = TextAnchor.UpperCenter;
        optionsLayout.childControlWidth = true;
        optionsLayout.childControlHeight = true;
        optionsLayout.childForceExpandWidth = true;
        optionsLayout.childForceExpandHeight = false;
        optionsLayout.padding = new RectOffset(10, 10, 10, 10);
        var optionsFitter = optionsContainerGo.AddComponent<ContentSizeFitter>();
        optionsFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // QuestionCardUI 필드 연결 (SerializedObject 사용)
        var questionCardSO = new SerializedObject(questionCardUI);
        questionCardSO.FindProperty("questionText").objectReferenceValue = questionTextComp;
        questionCardSO.FindProperty("optionsContainer").objectReferenceValue = optionsContainerGo.transform;

        // OptionButton Prefab 연결
        var optionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/OptionButton.prefab");
        if (optionPrefab != null)
            questionCardSO.FindProperty("optionButtonPrefab").objectReferenceValue = optionPrefab;
        else
            Debug.LogWarning("[SceneBuilder] OptionButton.prefab을 찾을 수 없습니다. QuestionCardUI.optionButtonPrefab을 수동으로 연결하세요.");

        questionCardSO.ApplyModifiedProperties();

        // ── 9. ResultCard ─────────────────────────────────────────
        var resultCardGo = CreateUIPanel(canvasGo.transform, "ResultCard",
            new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.88f));
        resultCardGo.AddComponent<CanvasGroup>();
        var resultCardUI = resultCardGo.AddComponent<ResultCardUI>();
        resultCardGo.SetActive(false);

        // TitleText
        var titleTextGo = CreateText(resultCardGo.transform, "TitleText",
            "진단 완료", 76, new Color(0.05f, 0.85f, 0.6f), FontStyle.Bold);
        var titleTextRect = titleTextGo.GetComponent<RectTransform>();
        titleTextRect.anchorMin = new Vector2(0f, 0.72f);
        titleTextRect.anchorMax = new Vector2(1f, 1f);
        titleTextRect.offsetMin = new Vector2(20f, 0f);
        titleTextRect.offsetMax = new Vector2(-20f, 0f);
        var titleTextComp = titleTextGo.GetComponent<Text>();
        titleTextComp.alignment = TextAnchor.MiddleCenter;
        titleTextComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        titleTextComp.verticalOverflow = VerticalWrapMode.Overflow;

        // SubtitleText
        var subtitleTextGo = CreateText(resultCardGo.transform, "SubtitleText",
            "혈자리 처방을 준비했습니다.", 48, new Color(0.85f, 0.87f, 0.90f), FontStyle.Normal);
        var subtitleTextRect = subtitleTextGo.GetComponent<RectTransform>();
        subtitleTextRect.anchorMin = new Vector2(0f, 0.52f);
        subtitleTextRect.anchorMax = new Vector2(1f, 0.70f);
        subtitleTextRect.offsetMin = new Vector2(20f, 0f);
        subtitleTextRect.offsetMax = new Vector2(-20f, 0f);
        var subtitleTextComp = subtitleTextGo.GetComponent<Text>();
        subtitleTextComp.alignment = TextAnchor.MiddleCenter;
        subtitleTextComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitleTextComp.verticalOverflow = VerticalWrapMode.Overflow;

        // LaunchARButton
        var launchBtnGo = CreateUIPanel(resultCardGo.transform, "LaunchARButton",
            new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.48f));
        var launchBtnImg = launchBtnGo.AddComponent<Image>();
        launchBtnImg.color = new Color(0.15f, 0.68f, 0.37f);
        var launchBtn = launchBtnGo.AddComponent<Button>();
        var launchBtnText = CreateText(launchBtnGo.transform, "Label",
            "✨ 마사지 가이드 시작", 46, new Color(0.1f, 0.1f, 0.1f), FontStyle.Bold);
        SetFullStretch(launchBtnText.GetComponent<RectTransform>());

        // ResetButton
        var resetBtnGo = CreateUIPanel(resultCardGo.transform, "ResetButton",
            new Vector2(0.15f, 0.10f), new Vector2(0.85f, 0.25f));
        var resetBtnImg = resetBtnGo.AddComponent<Image>();
        resetBtnImg.color = new Color(0.3f, 0.35f, 0.45f, 0.9f);
        var resetBtn = resetBtnGo.AddComponent<Button>();
        var resetBtnText = CreateText(resetBtnGo.transform, "Label",
            "다시 진단하기", 42, Color.white, FontStyle.Normal);
        SetFullStretch(resetBtnText.GetComponent<RectTransform>());

        // ResultCardUI 필드 연결
        var resultCardSO = new SerializedObject(resultCardUI);
        resultCardSO.FindProperty("titleText").objectReferenceValue = titleTextComp;
        resultCardSO.FindProperty("subtitleText").objectReferenceValue = subtitleTextComp;
        resultCardSO.FindProperty("launchARButton").objectReferenceValue = launchBtn;
        resultCardSO.FindProperty("resetButton").objectReferenceValue = resetBtn;
        resultCardSO.ApplyModifiedProperties();

        // ── 10. QuestionnaireContainer ────────────────────────────
        var containerGo = new GameObject("QuestionnaireContainer");
        containerGo.transform.SetParent(canvasGo.transform, false);
        var container = containerGo.AddComponent<QuestionnaireContainer>();

        // QuestionnaireContainer 필드 연결
        var containerSO = new SerializedObject(container);
        containerSO.FindProperty("questionCard").objectReferenceValue = questionCardUI;
        containerSO.FindProperty("resultCard").objectReferenceValue = resultCardUI;
        containerSO.FindProperty("loadingUI").objectReferenceValue = loadingGo.GetComponent<LoadingUI>();
        containerSO.FindProperty("backButton").objectReferenceValue = backBtn;
        containerSO.FindProperty("progressBar").objectReferenceValue = progressBarGo.GetComponent<ProgressBarUI>();
        containerSO.ApplyModifiedProperties();

        // ── 씬 저장 ───────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SceneBuilder] ✅ DiagnosisScene 빌드 완료: {scenePath}");

        // Build Settings에 씬 등록
        AddSceneToBuildSettings(scenePath);
    }

    // ─────────────────────────────────────────────────────────────
    //  AR 씬 빌드
    // ─────────────────────────────────────────────────────────────
    [MenuItem("MSG/Build ARScene")]
    public static void BuildARScene()
    {
        string scenePath = "Assets/Scenes/ARScene.unity";
        EnsureSceneDirectoryExists();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── 1. 필수 서비스 오브젝트 ──────────────────────────────
        CreateServiceObject<DataFetchService>("DataFetchService");
        CreateServiceObject<HandoffService>("HandoffService");
        CreateServiceObject<HandoffData>("HandoffData");

        // ── 2. EventSystem ────────────────────────────────────────
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── 3. AR Session ─────────────────────────────────────────
        // AR Foundation 컴포넌트는 패키지 설치 후 자동 인식됨
        // 에디터에서는 ARSession이 없어도 ARSceneController가 폴백 처리함
        var arSessionGo = new GameObject("AR Session");
        // ARSession 컴포넌트는 AR Foundation 패키지 설치 후 추가 가능
        // arSessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();

        var arOriginGo = new GameObject("XR Origin");
        var cameraOffsetGo = new GameObject("Camera Offset");
        cameraOffsetGo.transform.SetParent(arOriginGo.transform);

        var arCameraGo = new GameObject("AR Camera");
        arCameraGo.transform.SetParent(cameraOffsetGo.transform);
        var arCamera = arCameraGo.AddComponent<Camera>();
        arCamera.tag = "MainCamera";
        arCamera.clearFlags = CameraClearFlags.SolidColor;
        arCamera.backgroundColor = new Color(0.08f, 0.10f, 0.15f, 1f);

        // ── 4. AR 컨트롤러 오브젝트 ──────────────────────────────
        var arControllerGo = new GameObject("ARController");

        var cunCalibrator = arControllerGo.AddComponent<DynamicCunCalibrator>();
        var actionRenderer = arControllerGo.AddComponent<ARActionRenderer>();
        var deepLinkReceiver = arControllerGo.AddComponent<DeepLinkReceiver>();
        var arSceneController = arControllerGo.AddComponent<ARSceneController>();

        // ARSceneController 필드 연결
        var arControllerSO = new SerializedObject(arSceneController);
        arControllerSO.FindProperty("cunCalibrator").objectReferenceValue = cunCalibrator;
        arControllerSO.FindProperty("actionRenderer").objectReferenceValue = actionRenderer;
        arControllerSO.FindProperty("deepLinkReceiver").objectReferenceValue = deepLinkReceiver;
        arControllerSO.ApplyModifiedProperties();

        // ── 씬 저장 ───────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SceneBuilder] ✅ ARScene 빌드 완료: {scenePath}");

        AddSceneToBuildSettings(scenePath);
    }

    // ─────────────────────────────────────────────────────────────
    //  두 씬 한 번에 빌드
    // ─────────────────────────────────────────────────────────────
    [MenuItem("MSG/Build All Scenes")]
    public static void BuildAllScenes()
    {
        BuildDiagnosisScene();
        BuildARScene();
        Debug.Log("[SceneBuilder] ✅ 모든 씬 빌드 완료. Build Settings 확인 후 Android 빌드를 진행하세요.");
    }

    // ─────────────────────────────────────────────────────────────
    //  전체 프로젝트 초기 셋업 (권장 실행 순서)
    // ─────────────────────────────────────────────────────────────
    [MenuItem("MSG/[SETUP] Full Project Setup (Run This First)")]
    public static void FullProjectSetup()
    {
        Debug.Log("[SceneBuilder] === MSG 프로젝트 전체 셋업 시작 ===");

        // 1. AcupointDB 파싱 (CSV → ScriptableObject)
        if (System.Type.GetType("CSVParser") != null)
        {
            Debug.Log("[SceneBuilder] Step 1: AcupointDB CSV 파싱...");
            // CSVParser.ParseCSV() 는 MenuItem이므로 직접 호출 불가.
            // 사용자가 MSG > Parse Acupoints CSV 를 별도 실행해야 함.
            Debug.LogWarning("[SceneBuilder] ⚠️ MSG > Parse Acupoints CSV 를 먼저 실행하세요.");
        }

        // 2. OptionButton 프리팹 생성
        Debug.Log("[SceneBuilder] Step 2: OptionButton 프리팹 생성...");
        PrefabBuilder.BuildOptionButtonPrefab();

        // 3. 씬 빌드
        Debug.Log("[SceneBuilder] Step 3: 씬 빌드...");
        BuildAllScenes();

        Debug.Log("[SceneBuilder] === 셋업 완료 ===");
        Debug.Log("[SceneBuilder] 다음 단계:");
        Debug.Log("  1. MSG > Parse Acupoints CSV 실행 (AcupointDB.asset 생성)");
        Debug.Log("  2. File > Build Settings > Android 플랫폼 선택");
        Debug.Log("  3. Player Settings > Other Settings > Package Name 확인");
        Debug.Log("  4. Build And Run");
    }

    // ─────────────────────────────────────────────────────────────
    //  헬퍼 메서드
    // ─────────────────────────────────────────────────────────────
    private static void EnsureSceneDirectoryExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static GameObject CreateServiceObject<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        go.AddComponent<T>();
        return go;
    }

    private static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        return go;
    }

    private static GameObject CreateText(Transform parent, string name,
        string content, int fontSize, Color color, FontStyle style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();

        var text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;

        // 폰트 설정
        Font font = Resources.Load<Font>("LiberationSans");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null) text.font = font;

        return go;
    }

    private static void SetFullStretch(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8f, 4f);
        rect.offsetMax = new Vector2(-8f, -4f);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return; // 이미 등록됨
        }

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
        Debug.Log($"[SceneBuilder] Build Settings에 씬 추가: {scenePath}");
    }
}
#endif
