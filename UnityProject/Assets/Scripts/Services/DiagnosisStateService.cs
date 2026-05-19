using System;
using System.Collections.Generic;
using UnityEngine;
using MSG.Models;

namespace MSG.Services
{
    /// <summary>
    /// [WBS 1.3] DiagnosisStateService (Unity 포팅)
    /// RxJS BehaviorSubject 패턴을 C# event로 구현.
    /// 현재 노드 상태 + 뒤로가기 스택 관리 (결정론적 라우팅).
    /// LLM 개입 없음. 3-Second Rule 준수.
    /// </summary>
    public class DiagnosisStateService : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────
        public static DiagnosisStateService Instance { get; private set; }

        // ── State ─────────────────────────────────────────────────────
        private TreeNode _currentNode;
        private readonly Stack<TreeNode> _backStack = new Stack<TreeNode>();

        /// <summary>현재 노드 (BehaviorSubject 역할)</summary>
        public TreeNode CurrentNode => _currentNode;

        /// <summary>뒤로가기 가능 여부</summary>
        public bool CanGoBack => _backStack.Count > 0;

        /// <summary>현재 문답이 진행된 누적 단계 수</summary>
        public int StackCount => _backStack.Count;

        // ── Events (Observer Pattern) ──────────────────────────────────
        /// <summary>노드가 바뀔 때마다 발행 (Dumb UI가 구독)</summary>
        public event Action<TreeNode> OnNodeChanged;

        /// <summary>결과 리프 노드 도달 시 발행</summary>
        public event Action<DiagnosticResult> OnResultReached;

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

        /// <summary>
        /// 진단 시작: DataFetchService 로드 완료 후 최초 노드로 초기화.
        /// </summary>
        public void Initialize()
        {
            _backStack.Clear();
            var startNode = DataFetchService.Instance.GetNode("q_start");
            if (startNode == null)
            {
                Debug.LogError("[DiagnosisStateService] 시작 노드(q_start)를 찾을 수 없습니다.");
                return;
            }
            SetNode(startNode);
        }

        /// <summary>
        /// 옵션 선택: 다음 노드로 이동 (결정론적 라우팅).
        /// </summary>
        /// <param name="nextId">선택된 옵션의 nextId</param>
        public void SelectOption(string nextId)
        {
            // 리프 노드(결과) 처리
            if (nextId.StartsWith("res_"))
            {
                HandleResult(nextId);
                return;
            }

            var nextNode = DataFetchService.Instance.GetNode(nextId);
            if (nextNode == null)
            {
                Debug.LogWarning($"[DiagnosisStateService] 노드 '{nextId}'를 찾을 수 없음. 폴백 라우터에 위임.");
                FallbackRouter.Instance?.TryRoute(nextId, this);
                return;
            }

            // 현재 노드를 스택에 push (뒤로가기용)
            if (_currentNode != null)
                _backStack.Push(_currentNode);

            SetNode(nextNode);
        }

        /// <summary>
        /// 뒤로가기: 스택에서 이전 노드 복원.
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack)
            {
                Debug.Log("[DiagnosisStateService] 더 이상 뒤로 갈 수 없음. 시작 노드입니다.");
                return;
            }
            var prev = _backStack.Pop();
            SetNode(prev);
        }

        /// <summary>
        /// 진단 리셋: 처음부터 다시.
        /// </summary>
        public void Reset()
        {
            _backStack.Clear();
            Initialize();
        }

        // ── Private ────────────────────────────────────────────────────

        private void SetNode(TreeNode node)
        {
            _currentNode = node;
            Debug.Log($"[DiagnosisStateService] 현재 노드: {node.id}");
            OnNodeChanged?.Invoke(node);
        }

        private void HandleResult(string resultId)
        {
            var result = DataFetchService.Instance.GetResult(resultId);
            if (result == null)
            {
                Debug.LogError($"[DiagnosisStateService] 결과 '{resultId}'를 찾을 수 없습니다.");
                return;
            }
            Debug.Log($"[DiagnosisStateService] ✅ 진단 완료: {result.title} ({result.pts.Count}개 혈자리)");
            OnResultReached?.Invoke(result);
        }
    }
}
