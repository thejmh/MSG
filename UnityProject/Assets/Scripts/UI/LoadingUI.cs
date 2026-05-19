using UnityEngine;

namespace MSG.UI
{
    /// <summary>
    /// [WBS 1.5] Dumb Component: 로딩 화면 표시/숨김 전담.
    /// </summary>
    public class LoadingUI : MonoBehaviour
    {
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
