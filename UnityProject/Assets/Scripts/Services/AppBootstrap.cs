using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;
using MSG.Services;

namespace MSG.Services
{
    /// <summary>
    /// [AppBootstrap] 앱 시작 시 필수 서비스 자동 보장 + 크래시 방어막.
    ///
    /// 이 스크립트를 DiagnosisScene의 가장 첫 번째 오브젝트에 붙여두면
    /// 씬에 서비스 오브젝트가 없어도 자동으로 생성합니다.
    ///
    /// Script Execution Order: -100 (다른 스크립트보다 먼저 실행)
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AppBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            var go = new GameObject("AppBootstrap");
            go.AddComponent<AppBootstrap>();
            DontDestroyOnLoad(go);
            Debug.Log("[AppBootstrap] RuntimeInitializeOnLoadMethod를 통해 자동 부트스트랩 되었습니다.");
        }

        private void Awake()
        {
            Debug.Log("[AppBootstrap] 앱 부트스트랩 시작...");

            // ── 레거시 Input Module 강제 교체 (크래시 방지) ─────────────────────
            var oldModule = FindFirstObjectByType<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                var go = oldModule.gameObject;
                DestroyImmediate(oldModule);
                go.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[AppBootstrap] 레거시 StandaloneInputModule을 InputSystemUIInputModule로 교체했습니다.");
            }

            // ── Camera 보장 (No cameras rendering 방지) ─────────────────────
            if (Camera.main == null && FindFirstObjectByType<Camera>() == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f); // 어두운 배경 (UI 강조용)
                cam.orthographic = true;
                Debug.Log("[AppBootstrap] 씬에 카메라가 없어 Main Camera를 자동 생성했습니다.");
            }

            // ── 필수 서비스 자동 보장 ─────────────────────────────────
            EnsureService<DataFetchService>("DataFetchService");
            EnsureService<DiagnosisStateService>("DiagnosisStateService");
            EnsureService<HandoffService>("HandoffService");
            EnsureService<FallbackRouter>("FallbackRouter");
            EnsureService<HandoffData>("HandoffData");

            Debug.Log("[AppBootstrap] ✅ 모든 필수 서비스 준비 완료.");
        }

        private static T EnsureService<T>(string name) where T : MonoBehaviour
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null)
            {
                Debug.Log($"[AppBootstrap] {name} 이미 존재.");
                return existing;
            }

            var go = new GameObject(name);
            var service = go.AddComponent<T>();
            DontDestroyOnLoad(go);
            Debug.Log($"[AppBootstrap] {name} 자동 생성.");
            return service;
        }
    }
}
