using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;

public class ReplaceInputModule
{
    public static void FixInputModulesInAllScenes()
    {
        string[] scenes = new string[] {
            "Assets/Scenes/DiagnosisScene.unity",
            "Assets/Scenes/ARScene.unity"
        };

        foreach (var scenePath in scenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            bool changed = false;
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
                    Debug.Log($"Replaced StandaloneInputModule with InputSystemUIInputModule on {go.name} in {scenePath}");
                    EditorUtility.SetDirty(go);
                }
            }

            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Successfully saved {scenePath}");
            }
            else
            {
                Debug.Log($"No StandaloneInputModule found in {scenePath}");
            }
        }
    }
}
