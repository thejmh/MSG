using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using MSG.Models;

namespace MSG.Services
{
    /// <summary>
    /// [WBS 2.1 ~ 2.3] HandoffService (Data Bridge)
    /// 진단 결과를 최소 페이로드로 직렬화 후 AR 씬으로 안전하게 전달.
    ///
    /// Unity 단일 앱 방식에서는 딥링크 대신 인메모리 Singleton 전달 방식 사용.
    /// (딥링크 방식은 레거시 호환을 위해 유지)
    /// </summary>
    public class HandoffService : MonoBehaviour
    {
        public static HandoffService Instance { get; private set; }

        // AR 씬 이름 (Unity Build Settings에 등록 필요)
        private const string AR_SCENE_NAME = "ARScene";

        // Deep Link Scheme (레거시 호환 / 외부 앱 연동 시 사용)
        private const string DEEP_LINK_SCHEME = "msg-app://treat";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ─────────────────────────────────────────────────────────────
        //  [WBS 2.1] 페이로드 Minification
        //  결과 오브젝트 → {dId, pts:[{id, i, m}]} 최소 스키마 조립
        // ─────────────────────────────────────────────────────────────
        public HandoffPayload BuildPayload(DiagnosticResult result)
        {
            var pts = new System.Collections.Generic.List<MeridianPoint>();
            foreach (var p in result.pts)
            {
                // ⛔ X/Y/Z 좌표 등 불필요한 메타데이터 완벽 배제
                pts.Add(new MeridianPoint { id = p.id, i = p.i, m = p.m });
            }
            return new HandoffPayload { dId = result.id, pts = pts };
        }

        // ─────────────────────────────────────────────────────────────
        //  [WBS 2.2] Base64URL 직렬화 (레거시/외부 앱 연동용)
        //  RFC 4648 Base64URL 인코딩. 한글/특수문자 안전 처리.
        // ─────────────────────────────────────────────────────────────
        public string SerializeToBase64(HandoffPayload payload)
        {
            string json = JsonUtility.ToJson(payload);
            // 유니코드 안전: UTF-8 인코딩 후 Base64
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            string base64 = Convert.ToBase64String(bytes);

            // RFC 4648 URL-Safe 변환 (+→-, /→_, = 제거)
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public HandoffPayload DeserializeFromBase64(string encoded)
        {
            // URL-Safe 역변환
            string base64 = encoded.Replace('-', '+').Replace('_', '/');
            // 패딩 복원
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<HandoffPayload>(json);
        }

        // ─────────────────────────────────────────────────────────────
        //  [WBS 2.3] AR 가이드 시작 (Unity 단일 앱 방식)
        //  인메모리 전달 후 SceneManager로 AR 씬 전환.
        // ─────────────────────────────────────────────────────────────
        public void Handoff(DiagnosticResult result)
        {
            var payload = BuildPayload(result);

            // 인메모리 전달: HandoffData Singleton
            HandoffData.Instance.payload = payload;

            Debug.Log($"[HandoffService] ✨ AR 가이드 시작: {result.title} | {payload.pts.Count}개 혈자리");
            Debug.Log($"[HandoffService] (디버그) Base64: {SerializeToBase64(payload)}");

            SceneManager.LoadScene(AR_SCENE_NAME);
        }

        /// <summary>
        /// [레거시] 외부 Unity 앱으로 딥링크 핸드오프 (2-Phase 아키텍처 호환)
        /// </summary>
        public void LaunchViaDeepLink(DiagnosticResult result)
        {
            var payload = BuildPayload(result);
            string encoded = SerializeToBase64(payload);
            string deepLink = $"{DEEP_LINK_SCHEME}?p={encoded}";
            Debug.Log($"[HandoffService] 딥링크 발행: {deepLink}");
            Application.OpenURL(deepLink);
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Handoff 전용 페이로드 (직렬화 가능)
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public class HandoffPayload
    {
        /// <summary>진단 노드 ID (AR 씬 타이틀 표출용)</summary>
        public string dId;
        /// <summary>마사지 경로 혈자리 배열 (순서 = 경로)</summary>
        public System.Collections.Generic.List<MeridianPoint> pts;
    }
}
