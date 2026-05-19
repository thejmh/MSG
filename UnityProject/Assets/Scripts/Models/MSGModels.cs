using System;
using System.Collections.Generic;

namespace MSG.Models
{
    // ─────────────────────────────────────────────
    //  Decision Tree (진단 트리) — 라우팅용 노드
    // ─────────────────────────────────────────────

    /// <summary>문진 질문 노드의 선택지 (DiagnosisQuestion.options 원소)</summary>
    [Serializable]
    public class DiagnosisOption
    {
        public string text;
        public string nextId;
    }

    /// <summary>문진 트리의 단일 질문 노드</summary>
    [Serializable]
    public class DiagnosisQuestion
    {
        public string id;
        public string text;
        public List<DiagnosisOption> options;
    }

    [Serializable]
    public class TreeOption
    {
        public string text;
        public string nextId;
    }

    [Serializable]
    public class TreeNode
    {
        public string id;
        public string text;
        public bool isResult;
        public string resultId;
        public List<TreeOption> options;
    }

    [Serializable]
    public class TreeNodeList
    {
        public List<TreeNode> items;
    }

    // ─────────────────────────────────────────────
    //  Diagnostics (진단 결과)
    // ─────────────────────────────────────────────
    [Serializable]
    public class MeridianPoint
    {
        /// <summary>혈자리 DB 고유 ID (V11 기준)</summary>
        public int id;
        /// <summary>마사지 세기 (1: 강하게/초록, 0: 약하게/빨강)</summary>
        public int i;
        /// <summary>마사지 방법 (1: Press, 2: Rub, 3: Tap)</summary>
        public int m;
    }

    [Serializable]
    public class DiagnosticResult
    {
        public string id;
        public string title;
        public List<MeridianPoint> pts;
    }

    [Serializable]
    public class DiagnosticResultList
    {
        public List<DiagnosticResult> items;
    }

    // ─────────────────────────────────────────────
    //  Acupoint DB (혈자리 메타데이터)
    // ─────────────────────────────────────────────
    [Serializable]
    public class AcupointEntry
    {
        public int id;
        public string meridian;
        public string pointName;
        public string hanja;
        public int page;
        public string symptoms;
        public string priority;
        public string location;
    }
}
