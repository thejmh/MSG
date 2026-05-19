using System;
using System.Collections.Generic;
using UnityEngine;
using MSG.Services;

namespace MSG.AR
{
    /// <summary>
    /// [WBS 3.2] DeepLinkReceiver (Unity 단일 앱 포팅)
    /// 원래 Android Intent 감지 역할을 인메모리 HandoffData 수신으로 대체.
    /// 레거시 딥링크(msg-app://) 처리도 유지.
    /// </summary>
    public class DeepLinkReceiver : MonoBehaviour
    {
        public static DeepLinkReceiver Instance { get; private set; }

        [Header("수신된 페이로드 (런타임 모니터링용)")]
        public string receivedDiagnosticId;
        public List<ReceivedPoint> receivedPoints = new List<ReceivedPoint>();

        public event Action<HandoffPayload> OnPayloadReceived;

        private bool _processed = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // 우선순위 1: 인메모리 HandoffData (Unity 단일 앱)
            if (HandoffData.Instance?.payload != null && !_processed)
            {
                ProcessPayload(HandoffData.Instance.payload);
                return;
            }

            // 우선순위 2: 레거시 딥링크 (2-Phase 아키텍처 호환)
            Application.deepLinkActivated += OnDeepLinkActivated;

            // 앱 최초 실행 시 딥링크 확인
            if (!string.IsNullOrEmpty(Application.absoluteURL))
                OnDeepLinkActivated(Application.absoluteURL);
        }

        // ── 인메모리 수신 ────────────────────────────────────────────────
        private void ProcessPayload(HandoffPayload payload)
        {
            _processed = true;

            // ⛔ 웹의 문진 맥락 없이 순수 id, i, m 결과만 수신 확인
            receivedDiagnosticId = payload.dId;
            receivedPoints.Clear();
            foreach (var pt in payload.pts)
            {
                receivedPoints.Add(new ReceivedPoint { id = pt.id, i = pt.i, m = pt.m });
            }

            Debug.Log($"[DeepLinkReceiver] ✅ 페이로드 수신: {payload.dId} | {payload.pts.Count}개 혈자리");
            OnPayloadReceived?.Invoke(payload);
        }

        // ── 레거시 딥링크 ────────────────────────────────────────────────
        private void OnDeepLinkActivated(string url)
        {
            if (!url.StartsWith("msg-app://treat")) return;

            try
            {
                // System.Web 미사용 (Unity 비호환) → 수동 쿼리 파라미터 파싱
                string encoded = null;
                int queryStart = url.IndexOf('?');
                if (queryStart >= 0)
                {
                    string query = url.Substring(queryStart + 1);
                    foreach (var param in query.Split('&'))
                    {
                        var kv = param.Split('=');
                        if (kv.Length == 2 && kv[0] == "p")
                        {
                            encoded = Uri.UnescapeDataString(kv[1]);
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(encoded)) return;

                var payload = HandoffService.Instance.DeserializeFromBase64(encoded);
                ProcessPayload(payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeepLinkReceiver] 딥링크 파싱 실패: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }
    }

    [Serializable]
    public class ReceivedPoint
    {
        public int id;
        public int i;
        public int m;
    }
}
