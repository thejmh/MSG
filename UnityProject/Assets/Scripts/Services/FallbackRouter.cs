using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using MSG.Services;

namespace MSG.Services
{
    /// <summary>
    /// [WBS 1.4] FallbackRouter (텍스트 폴백 라우터)
    /// 정규식(Regex) 기반 키워드 추출 → 트리 특정 노드 강제 맵핑.
    /// 매칭 실패 시 시작 노드(q_start)로 복귀. 환각(Hallucination) 없음.
    /// </summary>
    public class FallbackRouter : MonoBehaviour
    {
        public static FallbackRouter Instance { get; private set; }

        // ── 키워드 → 노드 ID 매핑 테이블 ────────────────────────────────
        private static readonly List<(Regex pattern, string nodeId)> _routeMap
            = new List<(Regex, string)>
        {
            // 머리 관련
            (new Regex(@"두통|머리\s?아|편두통|이마", RegexOptions.IgnoreCase), "q_head_pain"),
            (new Regex(@"눈\s?피로|충혈|시력|눈\s?아", RegexOptions.IgnoreCase), "res_eye_fatigue"),
            (new Regex(@"어지|현기증|멀미", RegexOptions.IgnoreCase), "res_dizziness"),
            (new Regex(@"코막|비염|콧물", RegexOptions.IgnoreCase), "res_rhinitis"),

            // 목/어깨
            (new Regex(@"목\s?결|뒷목|목\s?아|경직", RegexOptions.IgnoreCase), "res_neck_stiffness"),
            (new Regex(@"어깨|오십견", RegexOptions.IgnoreCase), "q_shoulder"),
            (new Regex(@"인후|목소리|목\s?쉬|편도", RegexOptions.IgnoreCase), "res_throat"),

            // 호흡기
            (new Regex(@"기침|가래|해수", RegexOptions.IgnoreCase), "res_cough"),
            (new Regex(@"숨\s?차|호흡|답답", RegexOptions.IgnoreCase), "res_dyspnea"),
            (new Regex(@"두근|심장|심계항진", RegexOptions.IgnoreCase), "res_palpitation"),

            // 소화기
            (new Regex(@"소화|명치|체함|위", RegexOptions.IgnoreCase), "res_indigestion"),
            (new Regex(@"설사|복통|배\s?아", RegexOptions.IgnoreCase), "res_diarrhea"),
            (new Regex(@"변비", RegexOptions.IgnoreCase), "res_constipation"),
            (new Regex(@"구역|구토|메슥", RegexOptions.IgnoreCase), "res_nausea"),

            // 허리/등
            (new Regex(@"허리|요통", RegexOptions.IgnoreCase), "res_lower_back"),
            (new Regex(@"등\s?결|등\s?아|등\s?피로", RegexOptions.IgnoreCase), "res_upper_back"),
            (new Regex(@"좌골|신경통|다리\s?저", RegexOptions.IgnoreCase), "res_sciatica"),

            // 팔/손
            (new Regex(@"손목|팔꿈치", RegexOptions.IgnoreCase), "res_wrist_pain"),
            (new Regex(@"팔\s?저|팔\s?아|신경", RegexOptions.IgnoreCase), "res_arm_neuralgia"),
            (new Regex(@"손\s?차|수족냉|냉증", RegexOptions.IgnoreCase), "res_cold_hands"),

            // 다리/발
            (new Regex(@"무릎", RegexOptions.IgnoreCase), "res_knee_pain"),
            (new Regex(@"종아리|쥐\s?남|경련", RegexOptions.IgnoreCase), "res_calf_cramp"),
            (new Regex(@"발목|삐", RegexOptions.IgnoreCase), "res_ankle_pain"),
            (new Regex(@"다리\s?붓|부종|다리\s?피로", RegexOptions.IgnoreCase), "res_leg_fatigue"),
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 텍스트 입력 → 키워드 매칭 → 상태 서비스를 통해 노드 이동.
        /// 매칭 실패 시 q_start로 복귀.
        /// </summary>
        public void RouteByText(string inputText, DiagnosisStateService stateService)
        {
            foreach (var (pattern, nodeId) in _routeMap)
            {
                if (pattern.IsMatch(inputText))
                {
                    Debug.Log($"[FallbackRouter] '{inputText}' → 노드 '{nodeId}' 매칭 성공");
                    stateService.SelectOption(nodeId);
                    return;
                }
            }

            // 매칭 실패: 시작 노드로 복귀 (환각 없음 보장)
            Debug.Log($"[FallbackRouter] '{inputText}' 매칭 실패 → q_start로 복귀");
            stateService.Reset();
        }

        /// <summary>
        /// 내부 라우팅 실패 시 호출 (nextId 문자열 직접 처리).
        /// </summary>
        public void TryRoute(string nodeId, DiagnosisStateService stateService)
        {
            Debug.LogWarning($"[FallbackRouter] 알 수 없는 노드 ID '{nodeId}' → q_start로 복귀");
            stateService.Reset();
        }
    }
}
