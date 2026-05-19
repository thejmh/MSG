using UnityEngine;
using UnityEngine.UI;

namespace MSG.UI
{
    /// <summary>
    /// [WBS 1.6] Dumb Component: 진행률 바 렌더링 전담.
    /// [Self-Healing] 드래그 연결 누락 시 하위 요소를 자동 탐색하여 런타임 에러를 방지합니다.
    /// </summary>
    public class ProgressBarUI : MonoBehaviour
    {
        [Header("게이지 이미지 (드래그 누락 시 자동 조립)")]
        [SerializeField] private Image fillImage;

        private void Awake()
        {
            // 🛡️ 방어적 코드: 인스펙터 드래그 연결이 누락된 경우 하위에서 스스로 검색
            if (fillImage == null)
            {
                Transform fillTransform = transform.Find("Fill");
                if (fillTransform != null)
                {
                    fillImage = fillTransform.GetComponent<Image>();
                    Debug.Log($"[ProgressBarUI] 🛡️ '{gameObject.name}'의 하위 Fill 이미지를 자동으로 탐색하여 연결 완료!");
                }
            }

            // 게이지 바 초기 상태를 0%로 확실하게 비움
            UpdateDepth(0f);
        }

        public void UpdateDepth(float normalizedValue)
        {
            if (fillImage != null)
            {
                // 이미지 타입이 Filled인지 실시간 강제 보장
                if (fillImage.type != Image.Type.Filled)
                {
                    fillImage.type = Image.Type.Filled;
                    fillImage.fillMethod = Image.FillMethod.Horizontal;
                    fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                }

                fillImage.fillAmount = Mathf.Clamp01(normalizedValue);
            }
        }
    }
}
