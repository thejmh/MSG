#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 파일 기반 빌드 트리거.
/// "Temp/trigger_build.txt" 파일이 생성되면 자동으로 Android APK 빌드를 실행.
/// 외부 스크립트에서 해당 파일을 생성하여 빌드를 트리거할 수 있음.
/// </summary>
[InitializeOnLoad]
public static class AutoBuildTrigger
{
    private static readonly string TriggerFile =
        Path.Combine(Application.dataPath, "..", "Temp", "trigger_build.txt");

    static AutoBuildTrigger()
    {
        EditorApplication.update += CheckTrigger;
    }

    private static void CheckTrigger()
    {
        if (!File.Exists(TriggerFile)) return;

        // 트리거 파일 즉시 삭제 (중복 실행 방지)
        try { File.Delete(TriggerFile); } catch { }

        Debug.Log("[AutoBuildTrigger] 빌드 트리거 감지 → Android APK 빌드 시작");

        // 다음 프레임에 빌드 실행 (에디터 업데이트 루프 안전 처리)
        EditorApplication.delayCall += () =>
        {
            BuildScript.BuildAndroidMenu();
        };
    }
}
#endif
