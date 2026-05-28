using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;
using MSG.Services;

public class FixDiagnosisScene
{
    public static void Fix()
    {
        string scenePath = "Assets/Scenes/DiagnosisScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        bool changed = false;

        // 1. Fix Input Module
        var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        foreach (var es in eventSystems)
        {
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                GameObject go = standalone.gameObject;
                Object.DestroyImmediate(standalone, true);
                go.AddComponent<InputSystemUIInputModule>();
                changed = true;
                Debug.Log($"Replaced Input Module on {go.name}");
                EditorUtility.SetDirty(go);
            }
        }

        // 2. Fix Camera
        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f); // Dark background
            cam.orthographic = true;
            changed = true;
            Debug.Log("Added Main Camera");
            EditorUtility.SetDirty(camGo);
        }

        // 3. Ensure AppBootstrap
        var bootstrap = Object.FindFirstObjectByType<AppBootstrap>();
        if (bootstrap == null)
        {
            GameObject bsGo = new GameObject("AppBootstrap");
            bsGo.AddComponent<AppBootstrap>();
            changed = true;
            Debug.Log("Added AppBootstrap");
            EditorUtility.SetDirty(bsGo);
        }

        if (changed)
        {
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Successfully saved {scenePath}");
        }
        else
        {
            Debug.Log($"No changes needed in {scenePath}");
        }
    }
}
