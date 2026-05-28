// ─────────────────────────────────────────────────────────────────────────
//  DiagnosisManager.cs — 레거시 (비활성화됨)
//
//  ⚠️  이 스크립트는 초기 프로토타입입니다. 현재 아키텍처에서는 사용하지 않습니다.
//
//  정식 아키텍처:
//    - DataFetchService + DiagnosisStateService + HandoffService (Scripts/Services/)
//    - QuestionnaireContainer + QuestionCardUI + ResultCardUI (Scripts/UI/)
//
//  이 파일을 씬에 추가하지 마세요. 추가 시 NullReferenceException 크래시 발생.
// ─────────────────────────────────────────────────────────────────────────

// 전체 클래스를 LEGACY_DISABLED 심볼로 비활성화
// (삭제 대신 보존 — 레거시 참조용)
#if LEGACY_DIAGNOSIS_MANAGER_ENABLED

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MSG.Models;
using MSG.Services;

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

        HandoffData handoff = FindFirstObjectByType<HandoffData>();
        if (handoff == null)
        {
            var obj = new GameObject("HandoffData");
            handoff = obj.AddComponent<HandoffData>();
        }

        handoff.payload = new HandoffPayload
        {
            dId = res.id,
            pts = res.pts
        };

        Debug.Log($"[DiagnosisManager] 진단 완료: {res.title} → AR 씬 전환");
        SceneManager.LoadScene("ARScene");
    }
}

#endif
