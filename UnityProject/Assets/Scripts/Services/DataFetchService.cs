using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSG.Models;

namespace MSG.Services
{
    /// <summary>
    /// [WBS 1.2] DataFetchService (Unity 포팅)
    /// Resources 폴더의 정적 JSON을 최초 1회 메모리에 로드.
    /// 외부 API / 네트워크 통신 없음. Zero-Cost 원칙 준수.
    /// </summary>
    public class DataFetchService : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────
        public static DataFetchService Instance { get; private set; }

        // ── 로드된 데이터 (In-Memory Cache) ────────────────
        public List<TreeNode> TreeNodes { get; private set; }
        public List<DiagnosticResult> DiagnosticResults { get; private set; }
        public List<AcupointEntry> Acupoints { get; private set; }

        public bool IsLoaded { get; private set; } = false;

        // ── 이벤트 ─────────────────────────────────────────
        public event System.Action OnDataLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(LoadAllData());
        }

        /// <summary>
        /// Resources 폴더의 JSON 3종을 비동기로 로드.
        /// 완료 시 OnDataLoaded 이벤트 발행.
        /// </summary>
        private IEnumerator LoadAllData()
        {
            float startTime = Time.realtimeSinceStartup;

            // 1. Decision Tree
            TextAsset treeAsset = Resources.Load<TextAsset>("decision-tree");
            if (treeAsset != null)
            {
                TreeNodes = JsonHelper.FromJsonArray<TreeNode>(treeAsset.text);
                Debug.Log($"[DataFetchService] decision-tree.json 로드 완료: {TreeNodes.Count}개 노드");
            }
            else
            {
                Debug.LogError("[DataFetchService] decision-tree.json 파일을 찾을 수 없습니다.");
            }
            yield return null;

            // 2. Diagnostics Results
            TextAsset diagAsset = Resources.Load<TextAsset>("diagnostics");
            if (diagAsset != null)
            {
                DiagnosticResults = JsonHelper.FromJsonArray<DiagnosticResult>(diagAsset.text);
                Debug.Log($"[DataFetchService] diagnostics.json 로드 완료: {DiagnosticResults.Count}개 패키지");
            }
            else
            {
                Debug.LogError("[DataFetchService] diagnostics.json 파일을 찾을 수 없습니다.");
            }
            yield return null;

            // 3. Acupoints CSV (이미 파싱된 ScriptableObject 우선, 없으면 직접 파싱)
            AcupointDB db = Resources.Load<AcupointDB>("AcupointDB");
            if (db != null)
            {
                Acupoints = new List<AcupointEntry>();
                foreach (var a in db.acupoints)
                {
                    Acupoints.Add(new AcupointEntry
                    {
                        id = a.id,
                        meridian = a.meridian,
                        pointName = a.pointName,
                        hanja = a.hanja,
                        symptoms = a.symptoms,
                        priority = a.priority,
                        location = a.location
                    });
                }
                Debug.Log($"[DataFetchService] AcupointDB 로드 완료: {Acupoints.Count}개 혈자리");
            }
            else
            {
                Debug.LogWarning("[DataFetchService] AcupointDB.asset 없음. Editor 메뉴 MSG > Parse Acupoints CSV 를 먼저 실행하세요.");
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[DataFetchService] 전체 로드 완료 ({elapsed * 1000:F1}ms) - 3초 룰 준수: {elapsed < 3f}");

            IsLoaded = true;
            OnDataLoaded?.Invoke();
        }

        /// <summary>
        /// ID로 트리 노드를 즉시 반환 (O(n), 소규모 트리 기준 충분).
        /// </summary>
        public TreeNode GetNode(string id)
        {
            return TreeNodes?.Find(n => n.id == id);
        }

        /// <summary>
        /// 결과 ID로 진단 결과 패키지 즉시 반환.
        /// </summary>
        public DiagnosticResult GetResult(string resultId)
        {
            return DiagnosticResults?.Find(r => r.id == resultId);
        }

        /// <summary>
        /// 혈자리 ID로 메타데이터 즉시 반환.
        /// </summary>
        public AcupointEntry GetAcupoint(int id)
        {
            return Acupoints?.Find(a => a.id == id);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  JsonHelper: Unity 기본 JsonUtility로 배열 파싱
    // ─────────────────────────────────────────────────────────────────
    public static class JsonHelper
    {
        public static List<T> FromJsonArray<T>(string json)
        {
            string wrapped = $"{{\"items\":{json}}}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper?.items != null ? new List<T>(wrapper.items) : new List<T>();
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public List<T> items;
        }
    }
}
