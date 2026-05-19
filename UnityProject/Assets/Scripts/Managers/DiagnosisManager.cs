// ─────────────────────────────────────────────────────────────────────────
//  DiagnosisManager.cs — 초기 프로토타입 (레거시 호환)
//
//  ⚠️  정식 아키텍처: DataFetchService + DiagnosisStateService + HandoffService
//       (Scripts/Services/ 폴더 참조)
//  모든 데이터 모델은 MSG.Models (MSGModels.cs)에 있습니다.
// ─────────────────────────────────────────────────────────────────────────
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MSG.Models;    // DiagnosisQuestion, DiagnosisOption, DiagnosticResult, MeridianPoint
using MSG.Services;  // HandoffData, HandoffPayload, JsonHelper

public class DiagnosisManager : MonoBehaviour
{
    public Text questionText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    private List<DiagnosisQuestion> questions;
    private List<DiagnosticResult> results;

    void Start()
    {
        LoadData();
        ShowQuestion("q_start");
    }

    void LoadData()
    {
        TextAsset treeAsset = Resources.Load<TextAsset>("decision-tree");
        TextAsset diagAsset = Resources.Load<TextAsset>("diagnostics");

        if (treeAsset != null)
            questions = JsonHelper.FromJsonArray<DiagnosisQuestion>(treeAsset.text);

        if (diagAsset != null)
            results = JsonHelper.FromJsonArray<DiagnosticResult>(diagAsset.text);
    }

    public void ShowQuestion(string nodeId)
    {
        if (nodeId.StartsWith("res_"))
        {
            HandleResult(nodeId);
            return;
        }

        DiagnosisQuestion q = questions?.Find(x => x.id == nodeId);
        if (q == null) return;

        questionText.text = q.text;

        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);

        foreach (var opt in q.options)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
            var label = btnObj.GetComponentInChildren<Text>();
            if (label != null) label.text = opt.text;

            var btn = btnObj.GetComponent<Button>();
            string nextId = opt.nextId;
            btn.onClick.AddListener(() => ShowQuestion(nextId));
        }
    }

    void HandleResult(string resId)
    {
        DiagnosticResult res = results?.Find(x => x.id == resId);
        if (res == null) return;

        // ✅ FindFirstObjectByType 사용 (CS0618 경고 해소)
        HandoffData handoff = FindFirstObjectByType<HandoffData>();
        if (handoff == null)
        {
            var obj = new GameObject("HandoffData");
            handoff = obj.AddComponent<HandoffData>();
        }

        // MSG.Services.HandoffData.payload 사용 (CS1061 에러 해소)
        handoff.payload = new HandoffPayload
        {
            dId = res.id,
            pts = res.pts
        };

        Debug.Log($"[DiagnosisManager] 진단 완료: {res.title} → AR 씬 전환");
        SceneManager.LoadScene("ARScene");
    }
}
// ※ DiagnosisOption / DiagnosisQuestion 정의 제거 → MSG.Models (MSGModels.cs)로 통합
