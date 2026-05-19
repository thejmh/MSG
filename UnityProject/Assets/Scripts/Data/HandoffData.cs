using UnityEngine;
using MSG.Models; // MeridianPoint

namespace MSG.Services
{
    /// <summary>
    /// [WBS 2.3] HandoffData Singleton
    /// 진단 씬 ➡️ AR 씬 전환 간 HandoffPayload를 인메모리로 보존.
    /// [Self-Healing Singleton] 데이터 소실 및 참조 에러를 방지하기 위해 접근 시 자동 오브젝트 생성을 지원합니다.
    /// </summary>
    public class HandoffData : MonoBehaviour
    {
        private static HandoffData _instance;

        public static HandoffData Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 🛡️ [자가 치료] 씬에 존재하지 않을 시 게임오브젝트를 즉석 동적 생성
                    var go = GameObject.Find("HandoffData");
                    if (go == null)
                    {
                        go = new GameObject("HandoffData");
                    }
                    _instance = go.GetComponent<HandoffData>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<HandoffData>();
                    }
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>진단 결과 페이로드 (최소 스키마: dId + pts)</summary>
        public HandoffPayload payload;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
