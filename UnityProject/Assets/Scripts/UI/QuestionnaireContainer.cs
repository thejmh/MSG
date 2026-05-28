using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MSG.Models;
using MSG.Services;

namespace MSG.UI
{
    /// <summary>
    /// [WBS 1.5] Smart Component: QuestionnaireContainer
    /// [Self-Healing Engine v2] 
    /// 1. 컴포넌트가 누락된 채 이름만 매칭되어도 코드가 강제로 스크립트를 수색/추가하여 결합합니다.
    /// 2. StackCount 프로퍼티를 활용하여 0%부터 점진적으로 늘어나는 프로그레시브 모션을 보장합니다.
    /// </summary>
    public class QuestionnaireContainer : MonoBehaviour
    {
        [Header("Dumb UI References (누락 시 자동으로 수색 및 강제 부착됨)")]
        [SerializeField] private QuestionCardUI questionCard;
        [SerializeField] private ResultCardUI resultCard;
        [SerializeField] private LoadingUI loadingUI;
        [SerializeField] private Button backButton;
        [SerializeField] private ProgressBarUI progressBar;

        private DiagnosisStateService _state;
        private int _totalDepth = 3; // 평균 질문 깊이 추정치

        private void Awake()
        {
            // 🛡️ [철벽 자동화 엔진] 스크립트 컴포넌트가 붙어있지 않아도, 이름이 발견되면 자동으로 부착 및 배선
            Transform canvas = transform.parent;
            if (canvas != null)
            {
                questionCard = GetOrAddSmart<QuestionCardUI>(canvas, "QuestionCard");
                resultCard = GetOrAddSmart<ResultCardUI>(canvas, "ResultCard");
                loadingUI = GetOrAddSmart<LoadingUI>(canvas, "LoadingPanel");
                backButton = GetOrAddSmart<Button>(canvas, "BackButton");
                progressBar = GetOrAddSmart<ProgressBarUI>(canvas, "ProgressBar");
                
                Debug.Log("[QuestionnaireContainer] 🛡️ UI 자가 조립 및 스크립트 강제 바인딩 완료!");
            }
        }

        private void Start()
        {
            // 로딩 화면 표시
            loadingUI?.Show();
            questionCard?.Hide();
            resultCard?.Hide();

            // DataFetchService 인스턴스 null 방어
            if (DataFetchService.Instance == null)
            {
                Debug.LogError("[QuestionnaireContainer] DataFetchService.Instance가 null입니다. " +
                               "씬에 DataFetchService 오브젝트가 있는지 확인하세요.");
                return;
            }

            // DataFetchService 로드 완료 후 시작
            if (DataFetchService.Instance.IsLoaded)
                OnDataLoaded();
            else
                DataFetchService.Instance.OnDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded()
        {
            loadingUI?.Hide();

            // DiagnosisStateService 인스턴스 null 방어
            if (DiagnosisStateService.Instance == null)
            {
                Debug.LogError("[QuestionnaireContainer] DiagnosisStateService.Instance가 null입니다. " +
                               "씬에 DiagnosisStateService 오브젝트가 있는지 확인하세요.");
                return;
            }

            // 상태 서비스 구독
            _state = DiagnosisStateService.Instance;
            _state.OnNodeChanged += OnNodeChanged;
            _state.OnResultReached += OnResultReached;

            // 뒤로가기 버튼 리스너 등록 및 초기 상태 비활성화
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => _state.GoBack());
                backButton.gameObject.SetActive(false);
            }

            _state.Initialize();
        }

        // ── State 구독 콜백 ──────────────────────────────────────
        private void OnNodeChanged(TreeNode node)
        {
            resultCard?.Hide();
            questionCard?.Show(node);
            backButton?.gameObject.SetActive(_state.CanGoBack);
            
            // 💡 프로그레스바 실시간 점진적 성장 보장!
            if (progressBar != null && _state != null)
            {
                // CanGoBack이 참이라는 것은 최소 1단계 이상 들어왔다는 것
                float fillAmount = _state.CanGoBack
                    ? Mathf.Min(_state.StackCount / (float)_totalDepth, 0.9f)
                    : 0f;
                progressBar.UpdateDepth(fillAmount);
                Debug.Log($"[ProgressBar] 진행도 업데이트: {fillAmount * 100f}% (누적 문답수: {_state.StackCount})");
            }
        }

        private void OnResultReached(DiagnosticResult result)
        {
            questionCard?.Hide();
            backButton?.gameObject.SetActive(false);
            resultCard?.Show(result);
            
            if (progressBar != null)
            {
                progressBar.UpdateDepth(1f); // 완료 시 100% 꽉 채움
            }
        }

        private void OnDestroy()
        {
            if (_state != null)
            {
                _state.OnNodeChanged -= OnNodeChanged;
                _state.OnResultReached -= OnResultReached;
            }
            if (DataFetchService.Instance != null)
                DataFetchService.Instance.OnDataLoaded -= OnDataLoaded;
        }

        /// <summary>
        /// 🛡️ 이름으로 오브젝트를 찾고, 컴포넌트가 없으면 즉석에서 강제 AddComponent하여 반환합니다.
        /// </summary>
        private T GetOrAddSmart<T>(Transform parent, string name) where T : Component
        {
            string normalizedTarget = name.Replace(" ", "").Replace("_", "").ToLower();
            Transform foundTransform = null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                string normalizedChild = child.name.Replace(" ", "").Replace("_", "").ToLower();
                if (normalizedChild == normalizedTarget)
                {
                    foundTransform = child;
                    break;
                }
            }

            if (foundTransform == null)
            {
                foundTransform = parent.Find(name);
            }

            if (foundTransform != null)
            {
                T comp = foundTransform.GetComponent<T>();
                if (comp == null)
                {
                    comp = foundTransform.gameObject.AddComponent<T>();
                    Debug.Log($"[QuestionnaireContainer] 🛡️ '{foundTransform.name}'에 누락된 컴포넌트 [{typeof(T).Name}]를 자동으로 복구/부착했습니다!");
                }
                return comp;
            }

            return null;
        }
    }
}
