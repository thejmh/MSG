#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    private const string APK_OUTPUT = "msg.apk";

    // ── Unity 메뉴에서 직접 실행 ──────────────────────────────────
    [MenuItem("MSG/Build Android APK")]
    public static void BuildAndroidMenu()
    {
        Debug.Log("[BuildScript] Android APK 빌드 시작...");
        bool result = BuildAndroidInternal(exitOnFinish: false);
        if (result)
            EditorUtility.DisplayDialog("빌드 완료", $"APK 생성 성공!\n경로: {Path.GetFullPath(APK_OUTPUT)}", "확인");
        else
            EditorUtility.DisplayDialog("빌드 실패", "빌드 중 오류가 발생했습니다.\nConsole 창을 확인하세요.", "확인");
    }

    // ── 배치 모드 진입점 (-executeMethod 용, void 반환 필수) ──────
    public static void BuildAndroid()
    {
        BuildAndroidInternal(exitOnFinish: true);
    }

    // ── 실제 빌드 로직 ────────────────────────────────────────────
    private static bool BuildAndroidInternal(bool exitOnFinish)
    {
        // 씬 목록 수집 (Build Settings 등록 씬 우선, 없으면 파일 직접 탐색)
        var sceneList = new System.Collections.Generic.List<string>();

        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.enabled && File.Exists(s.path))
                sceneList.Add(s.path);
        }

        // Build Settings에 씬이 없으면 Assets/Scenes 폴더 탐색
        if (sceneList.Count == 0)
        {
            string[] found = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
            foreach (var f in found)
                sceneList.Add(f.Replace("\\", "/"));
        }

        if (sceneList.Count == 0)
        {
            Debug.LogError("[BuildScript] 빌드할 씬이 없습니다. Build Settings에 씬을 추가하세요.");
            if (exitOnFinish) EditorApplication.Exit(1);
            return false;
        }

        Debug.Log($"[BuildScript] 빌드 씬 목록 ({sceneList.Count}개):");
        foreach (var s in sceneList) Debug.Log($"  - {s}");

        var options = new BuildPlayerOptions
        {
            scenes           = sceneList.ToArray(),
            locationPathName = APK_OUTPUT,
            target           = BuildTarget.Android,
            options          = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] ✅ 빌드 성공! 크기: {summary.totalSize / 1024 / 1024}MB | 경로: {Path.GetFullPath(APK_OUTPUT)}");
            if (exitOnFinish) EditorApplication.Exit(0);
            return true;
        }
        else
        {
            Debug.LogError($"[BuildScript] ❌ 빌드 실패: {summary.result} | 에러 수: {summary.totalErrors}");
            if (exitOnFinish) EditorApplication.Exit(1);
            return false;
        }
    }
}
#endif
